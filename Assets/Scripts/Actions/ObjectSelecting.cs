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
                    StopMovingObjects(deselect: false);
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
                                          where multiToolBounds.Intersects(item.GetComponent<Collider>().bounds)
                                          select item;
                foreach (GameObject intersectingObject in intersectingObjects)
                {
                    bool willBeSelected = toBeSelected.Contains(intersectingObject);
                    bool willBeRemoved = toBeRemoved.Contains(intersectingObject);
                    if (!willBeSelected && !willBeRemoved)
                    {
                        if (SelectedObjects.Contains(intersectingObject))
                        {
                            intersectingObject.GetComponent<LineRenderer>().material = new Material(Shader.Find("Particles/Additive"));
                            toBeRemoved.Add(intersectingObject);
                        }
                        else
                        {
                            intersectingObject.GetComponent<LineRenderer>().material = new Material(Shader.Find("Particles/Multiply"));
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
                GameObject newObj = new GameObject
                {
                    name = GlobalVars.LineName,
                    tag = GlobalVars.UniversalTag
                };
                newObj.transform.position = oldObj.transform.position;
                newObj.transform.rotation = oldObj.transform.rotation;

                var oldLineRenderer = oldObj.GetComponent<LineRenderer>();
                var newLineRenderer = newObj.AddComponent<LineRenderer>();

                newLineRenderer.numCapVertices = oldLineRenderer.numCapVertices;
                newLineRenderer.numCornerVertices = oldLineRenderer.numCornerVertices;
                newLineRenderer.positionCount = oldLineRenderer.positionCount;

                Vector3[] newPos = new Vector3[oldLineRenderer.positionCount];
                oldLineRenderer.GetPositions(newPos);
                newLineRenderer.SetPositions(newPos);

                newLineRenderer.useWorldSpace = false;
                newLineRenderer.material = oldLineRenderer.material;
                oldLineRenderer.material = new Material(Shader.Find("Particles/Additive"));
                newLineRenderer.startColor = oldLineRenderer.startColor;
                newLineRenderer.endColor = oldLineRenderer.endColor;
                newLineRenderer.startWidth = oldLineRenderer.startWidth;
                newLineRenderer.endWidth = oldLineRenderer.endWidth;

                newObj.AddComponent<MeshCollider>();
                newObj.GetComponent<MeshCollider>().sharedMesh = oldLineRenderer.GetComponent<MeshCollider>().sharedMesh;

                toBeCopied.Add(newObj);
            }
            SelectedObjects.Clear();
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
                obj.GetComponent<LineRenderer>().material = new Material(Shader.Find("Particles/Additive")); // todo replace with previous material
            }
            SelectedObjects.Clear();
        }
    }
}