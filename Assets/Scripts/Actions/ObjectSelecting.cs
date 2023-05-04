﻿using Assets.Scripts.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Actions
{
    public class ObjectSelecting : Action
    {
        private static HashSet<GameObject> SelectedObjects { get; set; } = new HashSet<GameObject>();
        private readonly HashSet<GameObject> toBeSelected = new HashSet<GameObject>();
        private readonly HashSet<GameObject> toBeRemoved = new HashSet<GameObject>();
        private GameObject[] gameObjects;
        private SelectionState CurrentState = SelectionState.STANDBY;
        private SelectionState ToolState = SelectionState.SELECTING;

        private enum SelectionState
        {
            STANDBY,
            SELECTING,
            COPYING,
            MOVING
        }

        public void SetStateCopying()
        {
            ToolState = SelectionState.COPYING;
        }

        public void SetStateMoving()
        {
            ToolState = SelectionState.MOVING;
        }

        public void SetStateChangeColor()
        {
            ChangeSelectionColor();
            ToolState = SelectionState.STANDBY;
        }

        public override void HandleTriggerDown()
        {
            switch (ToolState)
            {
                case SelectionState.SELECTING: // select objects
                    CurrentState = SelectionState.SELECTING;
                    gameObjects = GameObject.FindGameObjectsWithTag(GlobalVars.UniversalTag);
                    break;
                case SelectionState.COPYING: // copy selected objects relative to flystick position
                    CopySelection();
                    CurrentState = SelectionState.MOVING;
                    MoveObjects();
                    break;
                case SelectionState.MOVING: // move selected objects relative to flystick position
                    CurrentState = SelectionState.MOVING;
                    MoveObjects();
                    break;
                default: // catching bugs
                    ToolState = SelectionState.SELECTING;
                    break;
            }
        }

        public override void HandleTriggerUp()
        {
            switch (CurrentState)
            {
                case SelectionState.SELECTING:
                    ToolState = SelectionState.SELECTING;
                    CurrentState = SelectionState.STANDBY;
                    SelectedObjects.UnionWith(toBeSelected);
                    SelectedObjects.ExceptWith(toBeRemoved);
                    toBeSelected.Clear();
                    toBeRemoved.Clear();
                    break;

                case SelectionState.MOVING:
                    ToolState = SelectionState.SELECTING;
                    CurrentState = SelectionState.STANDBY;
                    StopMovingObjects(deselect: true);
                    break;

                default:
                    break;
            }
        }

        public override void Init()
        {
            // Nothing happens
        }

        public override void Update()
        {
            if (CurrentState == SelectionState.SELECTING)
            {
                Bounds multiToolBounds = FlystickManager.Instance.MultiTool.GetComponent<Collider>().bounds;
                var intersectingObjects = from item
                                          in gameObjects
                                          where ((item.GetComponent<Collider>() != null) && (multiToolBounds.Intersects(item.GetComponent<Collider>().bounds)))
                                          select item;
                foreach (GameObject item in intersectingObjects)
                {
                    GameObject intersectingObject;
                    if (item.transform.parent != null)
                    {
                        intersectingObject = item.transform.parent.gameObject;
                    }
                    else
                    {
                        intersectingObject = item;
                    }
                    bool willBeSelected = toBeSelected.Contains(intersectingObject);
                    bool willBeRemoved = toBeRemoved.Contains(intersectingObject);
                    if (!willBeSelected && !willBeRemoved)
                    {
                        if (SelectedObjects.Contains(intersectingObject))
                        {
                            changeColorToDefault(intersectingObject);
                            toBeRemoved.Add(intersectingObject);
                        }
                        else
                        {
                            changeColorToSelected(intersectingObject);
                            toBeSelected.Add(intersectingObject);
                        }
                    }
                }
            }
        }

        public override void Finish()
        {
            // Nothing happens
        }

        public void DeleteSelection()
        {
            foreach (var selectedObject in SelectedObjects)
            {
                Object.Destroy(selectedObject);
            }
            SelectedObjects.Clear();
        }

        public void CopySelection()
        {
            var toBeCopied = new HashSet<GameObject>();
            foreach (var oldObj in SelectedObjects)
            {
                GameObject newObj;
                newObj = Object.Instantiate(oldObj);
                newObj.name = oldObj.name;
                toBeCopied.Add(newObj);
            }
            DeselectAll();
            SelectedObjects.UnionWith(toBeCopied);
        }

        internal void MoveObjects()
        {
            foreach (var obj in SelectedObjects)
            {
                obj.transform.parent = FlystickManager.Instance.MultiTool.transform;
            }
        }

        private void StopMovingObjects(bool deselect = true)
        {
            foreach (var obj in SelectedObjects)
            {
                obj.transform.parent = null;
            }
            if (deselect) DeselectAll();
        }

        public static void DeselectAll()
        {
            foreach (var obj in SelectedObjects)
            {
                changeColorToDefault(obj);
            }
            SelectedObjects.Clear();
        }

        public void ChangeSelectionColor()
        {
            foreach (var obj in SelectedObjects)
            {
                obj.GetComponent<Renderer>().material.color = GameManager.Instance.CurrentColor;
            }
            SelectedObjects.Clear();
        }

        public void ChangeSelectionScale(Vector3 scale)
        {
            foreach (var obj in SelectedObjects)
            {
                obj.transform.localScale = scale;
            }
        }

        private static void changeColorToDefault(GameObject obj)
        {
            obj.GetComponent<Renderer>().material.color -= new Color(0f, 0f, 0f, 0.75f);
        }

        private void changeColorToSelected(GameObject obj)
        {
            obj.GetComponent<Renderer>().material.color += new Color(0f, 0f, 0f, 0.5f);
        }
    }
}