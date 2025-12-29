using System.Collections.Generic;
using UnityEngine;

namespace Shears.Loading
{
    [System.Serializable]
    public class LoadRequest
    {
        [SerializeField] private bool opensLoadingScreen;
        [SerializeField] private bool pausesGame;
        [SerializeField] private LoadAction[] actions;

        public bool OpensLoadingScreen { get => opensLoadingScreen; set => opensLoadingScreen = value; }
        public bool PausesGame { get => pausesGame; set => pausesGame = value; }

        public IReadOnlyList<LoadAction> Actions => actions;

        public LoadRequest(params LoadAction[] actions)
        {
            opensLoadingScreen = false;
            pausesGame = false;
            this.actions = actions;
        }
    }
}
