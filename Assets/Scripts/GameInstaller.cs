using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private CellManager _cellManager;
    [SerializeField, Space(15f)]
    private CellPaletteSettings _cellPaletteSettings;


    public override void InstallBindings()
    {

        Container.BindInstance(_cellManager).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();

        _cellManager.OnCellClicked += CellManagerOnCellClicked;

        Controls controls = new Controls();
        controls.Enable(); // Обязательно включаем!
        Container.Bind<Controls>().FromInstance(controls).AsSingle();
        Container.Bind<SceneController>().AsSingle();
        
    }

    private void CellManagerOnCellClicked(Cell obj)
    {
        obj.SetSelect(_cellPaletteSettings.SelectCell);
    }
}
