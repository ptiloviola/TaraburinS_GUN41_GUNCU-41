using System;
using System.Collections.Generic;
using Gameplay.Core.Services;

namespace Gameplay.Campaign.Data 
{
    public class RunProgressModel
    {
        private readonly RunSaveService _runSaveService;
        private RunSaveData _saveData;

        public RunMapModel CurrentMap { get; set; }
        public MapNode CurrentNode { get; set; }


        public event Action<MapNode> OnNodeChanged;
        public event Action<int> OnGoldChanged;

        public RunProgressModel(RunSaveService runSaveService)
        {
            _runSaveService = runSaveService;
        }

        public void LoadOrCreateData()
        {
            if (_runSaveService.HasSave())
            {
                _saveData = _runSaveService.LoadRun();
                
                CurrentNode = null; 
            }
            else
            {
                _saveData = new RunSaveData();
                _runSaveService.SaveRun(_saveData);
                
                CurrentMap = null; 
                CurrentNode = null; 
            }
        }

        public string CurrentNodeId => _saveData?.CurrentNodeId ?? string.Empty;
        public int CurrentRunDepth => _saveData?.CurrentRunDepth ?? 0;
        public int CurrentRunGold => _saveData?.CurrentRunGold ?? 0;
        public IReadOnlyList<string> PathHistory => _saveData?.PathHistory;
        public IReadOnlyList<string> ActiveRunItems => _saveData?.ActiveRunItems;


        public void MoveToNode(MapNode targetNode)
        {
            CurrentNode = targetNode;
            
            _saveData.CurrentNodeId = targetNode.Id;
            _saveData.CurrentRunDepth = targetNode.Depth;
            
            if (!_saveData.PathHistory.Contains(targetNode.Id))
            {
                _saveData.PathHistory.Add(targetNode.Id);
            }

            _runSaveService.SaveRun(_saveData);
            
            OnNodeChanged?.Invoke(targetNode);
        }

        public void AddGold(int amount)
        {
            _saveData.CurrentRunGold += amount;
            _runSaveService.SaveRun(_saveData);
            OnGoldChanged?.Invoke(_saveData.CurrentRunGold);
        }

        public void AddItem(string itemId)
        {
            if (!_saveData.ActiveRunItems.Contains(itemId))
            {
                _saveData.ActiveRunItems.Add(itemId);
                _runSaveService.SaveRun(_saveData);
            }
        }

        public bool IsNodeVisited(string nodeId)
        {
            return _saveData != null && _saveData.PathHistory.Contains(nodeId);
        }
    }
}