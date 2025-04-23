using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Overlays;
using Robust.Shared.Timing;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Content.Shared.Clothing;

namespace Content.Client.Overlays
{
    public sealed class WaterViewerHudSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

private ShaderInstance _waterViewerShader = null!;
private WaterViewerOverlay _waterViewerOverlay = null!;


    public WaterViewerHudSystem()
    {
        IoCManager.InjectDependencies(this);
    }

    public override void Initialize()
    {
        _waterViewerShader = _prototypeManager.Index<ShaderPrototype>("Cataracts").Instance();
        _waterViewerOverlay = new WaterViewerOverlay(_waterViewerShader, _entityManager, _player);
        
        SubscribeLocalEvent<WaterViewerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<WaterViewerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WaterBlockerComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<WaterBlockerComponent, GotUnequippedEvent>(OnUnequipped);
    }


    private void OnInit(Entity<WaterViewerComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity == ent)
        {
            _overlayMan.AddOverlay(_waterViewerOverlay);
        }
    }

    private void OnEquipped(Entity<WaterBlockerComponent> ent, ref GotEquippedEvent args)
    {
        EnsureComp<WaterBlockerComponent>(args.Equipee);
    }

    private void OnUnequipped(Entity<WaterBlockerComponent> ent, ref GotUnequippedEvent args)
    {
        _entityManager.RemoveComponent<WaterBlockerComponent>(args.Equipee);
    }

    private void OnShutdown(Entity<WaterViewerComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent)
        {
            _overlayMan.RemoveOverlay(_waterViewerOverlay);
        }
    }
}

   public sealed class WaterViewerOverlay : Overlay
{
    private readonly ShaderInstance _shaderInstance;
    private readonly IEntityManager _entityManager;
    private readonly IPlayerManager _playerManager;

    public WaterViewerOverlay(ShaderInstance shaderInstance, IEntityManager entityManager, IPlayerManager playerManager)
    {
        _shaderInstance = shaderInstance;
        _entityManager = entityManager;
        _playerManager = playerManager;
    }

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;

        if (!playerEntity.HasValue)
            return false;

        if (!_entityManager.TryGetComponent(playerEntity.Value, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;

        // Duplicate the shader instance to make it mutable
        var mutableShaderInstance = _shaderInstance.Duplicate();

        mutableShaderInstance.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        // Use the duplicated mutable shader
        handle.UseShader(mutableShaderInstance);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
}
