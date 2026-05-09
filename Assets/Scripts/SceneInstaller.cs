using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField]
    private Battlefield _battlefield;
    [SerializeField, Space(15f)]
    private CellPaletteSettings _cellPaletteSettings;

    [SerializeField]
    private BattleController _battleController;

    [SerializeField, Space(15f)] private UnitPaletteSettings _unitPaletteSettings;




    public override void InstallBindings()
    {

        Container.BindInstance(_battlefield).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();
        Container.BindInstance(_battleController).AsSingle();

        Controls controls = new Controls();
        controls.Enable();
        Container.Bind<Controls>().FromInstance(controls).AsSingle();
        Container.Bind<SceneController>().AsSingle();

        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<GameStatus>();
        Container.DeclareSignal<GameEvent>();
        Container.DeclareSignal<TurnChangedSignal>();
        Container.DeclareSignal<CheckSignal>();

        Container.Bind<ISharedData>().To<SingleSharedData>().AsSingle();

        Container.Bind<IGameplayCommand>().To<ChessCommand>().AsSingle();

        Container.Bind<ITurn>().To<OneByOneTurn>().AsSingle().WithArguments((IReadOnlyList<Team>)new List<Team> { Team.White, Team.Black});
        Container.BindInstance(_unitPaletteSettings).AsSingle();

        
    }

}
