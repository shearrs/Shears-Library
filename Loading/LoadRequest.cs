using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shears.Loading
{
    [System.Serializable]
    public struct LoadRequest
    {
        [SerializeField] private bool opensLoadingScreen;
        [SerializeField] private bool pausesGame;
        [SerializeField] private LoadAction[] actions;

        public bool OpensLoadingScreen { readonly get => opensLoadingScreen; set => opensLoadingScreen = value; }
        public bool PausesGame { readonly get => pausesGame; set => pausesGame = value; }

        public readonly IReadOnlyList<LoadAction> Actions => actions;

        public LoadRequest(params LoadAction[] actions)
        {
            opensLoadingScreen = false;
            pausesGame = false;
            this.actions = actions;
        }
    }
}
