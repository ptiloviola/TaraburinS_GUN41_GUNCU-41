using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private CellManager _cellManager;
    [SerializeField, Space(15f)]
    private CellPaletteSettings _cellPaletteSettings;

    private Unit _selectedUnit;


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

    private void CellManagerOnCellClicked(Cell clickedCell)
    {
        clickedCell.SetSelect(_cellPaletteSettings.SelectCell);

        // Unit myUnit = FindObjectOfType<Unit>();
        // if (myUnit != null)
        // {
        //     myUnit.Move(clickedCell);
        // }

        if (clickedCell.Unit != null)
        {
            if (_selectedUnit != null)
            {
                _selectedUnit.SetHighlight(false);
            }
            _selectedUnit = clickedCell.Unit;
            _selectedUnit.SetHighlight(true);
            Debug.Log($"<color=green> Selected Unit: {_selectedUnit}</color>");
        }
        else
        {
            if (_selectedUnit != null)
            {
                Debug.Log($"<color=yellow>Unit {_selectedUnit} moves to {clickedCell.transform.position}</color>");
                _selectedUnit.SetHighlight(false);
                _selectedUnit.Move(clickedCell);
                _selectedUnit = null;
            }
            else
            {
                Debug.Log("<color=red>You clicked on an empty cell, but no unit is selected!</color>");
            }
        }
    }
}
