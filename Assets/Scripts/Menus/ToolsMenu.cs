﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Managers;

namespace Assets.Scripts.Menus
{
    public class ToolsMenu : Menu
    {
        public const int ICON_COUNT = 9;
        public const float ICON_LOCAL_SCALE_X = 0.1f;
        public const int ICON_OFFSET_X = 1;
        public const float ICON_MARGIN_RATIO = 0.2f;

        public struct RowColumnCount
        {
            public int rowCount;
            public int columnCount;
        }

        private readonly MenuIcon colorPickingIcon = new MenuIcon(GameObject.Find("Color Picking"), GameManager.Instance.CurrentAction);

        public ToolsMenu()
        {
            icons.Add(colorPickingIcon);
        }

        //void instantiateIcons()
        //{
        //    var rowColumnCount = new RowColumnCount();
        //    determineRowAndColumnCount(ref rowColumnCount);

        //    float iconWithMarginHeight = (1f / rowColumnCount.rowCount);
        //    float iconWithMarginWidth = (1f / rowColumnCount.columnCount);

        //    float iconHeight = iconWithMarginHeight * (1f - ICON_MARGIN_RATIO);
        //    float iconWidth = iconWithMarginWidth * (1f - ICON_MARGIN_RATIO);

        //    float initialY = 0.5f - iconWithMarginHeight / 2;
        //    float initialZ = -0.5f + iconWithMarginWidth / 2;

        //    for (int i = 0; i < rowColumnCount.rowCount; i++)
        //    {
        //        for (int j = 0; j < rowColumnCount.columnCount; j++)
        //        {
        //            var instantiatedIcon = Instantiate(
        //                    icon,
        //                    new Vector3(0, 0, 0),
        //                    Quaternion.identity
        //                );
        //            instantiatedIcon.name = "Icon " + (icons.Count + 1).ToString();
        //            instantiatedIcon.transform.parent = gameObject.transform;
        //            instantiatedIcon.transform.localScale = new Vector3(ICON_LOCAL_SCALE_X, iconHeight, iconWidth);
        //            instantiatedIcon.transform.localEulerAngles = new Vector3(0, 0, 0);
        //            instantiatedIcon.transform.localPosition = new Vector3(
        //                ICON_OFFSET_X,
        //                initialY - i * iconWithMarginHeight,
        //                initialZ + j * iconWithMarginWidth
        //            );
        //            instantiatedIcon.AddComponent<MenuIconBehaviour>();

        //            icons.Add(instantiatedIcon);
        //            if (icons.Count == ICON_COUNT)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //}

        void determineRowAndColumnCount(ref RowColumnCount rowColumnCount)
        {
            while (true)
            {
                if (rowColumnCount.rowCount * rowColumnCount.columnCount >= ICON_COUNT)
                {
                    break;
                }
                rowColumnCount.rowCount++;
                if (rowColumnCount.rowCount * rowColumnCount.columnCount >= ICON_COUNT)
                {
                    break;
                }
                rowColumnCount.columnCount++;
            }
        }
    }
}