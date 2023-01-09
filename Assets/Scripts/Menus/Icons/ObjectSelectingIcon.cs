﻿using Assets.Scripts.Actions;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Menus.Icons
{
    public class ObjectSelectingIcon : MenuIcon
    {
        private readonly System.Action doWhenSelected;
        public ObjectSelectingIcon(GameObject icon, Action action, System.Action doWhenSelected) : base(icon, action)
        {
            this.doWhenSelected = doWhenSelected;
        }

        public override void Select()
        {
            SetDefaultColor();
            GameManager.Instance.changeCurrentAction(action);
            doWhenSelected();
        }
    }
}