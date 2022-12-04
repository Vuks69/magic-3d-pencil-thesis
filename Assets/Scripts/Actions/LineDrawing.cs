﻿using Assets.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Actions
{
    public class LineDrawing : Action
    {
        private bool drawing = false;
        private GameObject tool;
        private LineRenderer lineRenderer;
        private GameObject line;
        private Vector3 lastPosition;
        public float StrokeWidth { get; set; } = 0.01f;

        public override void Init()
        {
            tool = FlystickManager.Instance.MultiTool;
        }

        public override void HandleTriggerDown()
        {
            StartDrawing();
        }

        public override void HandleTriggerUp()
        {
            //throw new System.Exception();
            if (drawing)
            {
                drawing = false;
                if (lineRenderer.positionCount < 2)
                {
                    Object.Destroy(line);
                }
                else
                {
                    createCollider(line);
                }
            }
        }

        public override void Finish()
        {
            // Nothing happens
        }

        public override void Update()
        {
            if (drawing && Vector3.Distance(lastPosition, tool.transform.position) > 0.005f)
            {
                // once the flystick has moved away enough from last position, add new position
                // this is done to prevent adding 60 positions per second while drawing

                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, tool.transform.position - line.transform.position);
                lastPosition = tool.transform.position;
            }
        }

        private void StartDrawing()
        {
            if (!drawing)
            {
                // each line has to be its own object, as it can only have one renderer
                line = instantiateLine();
                drawing = true;
            }
        }

        private GameObject instantiateLine()
        {
            GameObject gameObject = new GameObject
            {
                name = GlobalVars.LineName,
                tag = GlobalVars.UniversalTag
            };
            gameObject.transform.position = tool.transform.position;

            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.numCapVertices = 1;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = false;

            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));    // todo add shader selection
            lineRenderer.startColor = GameManager.Instance.CurrentColor;
            lineRenderer.endColor = GameManager.Instance.CurrentColor;
            lineRenderer.startWidth = StrokeWidth;
            lineRenderer.endWidth = StrokeWidth;

            return gameObject;
        }

        public static void createCollider(GameObject line)
        {
            GameObject caret = new GameObject("Lines");
            LineRenderer _lineRenderer = line.GetComponent<LineRenderer>();
            List<Vector3> points = new List<Vector3>();
            Vector3 left, right;

            // For all but the last point
            for (var i = 0; i < _lineRenderer.positionCount - 1; i++)
            {
                caret.transform.position = _lineRenderer.GetPosition(i);
                caret.transform.LookAt(_lineRenderer.GetPosition(i + 1));
                right = caret.transform.position + line.transform.right * _lineRenderer.startWidth / 2;
                left = caret.transform.position - line.transform.right * _lineRenderer.startWidth / 2;
                points.Add(left);
                points.Add(right);
            }

            // Last point looks backwards and reverses
            caret.transform.position = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);
            caret.transform.LookAt(_lineRenderer.GetPosition(_lineRenderer.positionCount - 2));
            right = caret.transform.position - line.transform.right * _lineRenderer.startWidth / 2;
            left = caret.transform.position + line.transform.right * _lineRenderer.startWidth / 2;
            points.Add(left);
            points.Add(right);
            Object.Destroy(caret);
            Mesh mesh = drawMesh(points);
            var collider = line.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        public static Mesh drawMesh(List<Vector3> points)
        {
            Vector3[] vertices = new Vector3[points.Count];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = points[i];
            }

            int[] triangles = new int[((points.Count / 2) - 1) * 6];

            //Works on linear patterns tn = bn+c
            int position = 6;
            for (int i = 0; i < (triangles.Length / 6); i++)
            {
                triangles[i * position] = 2 * i;
                triangles[i * position + 3] = 2 * i;

                triangles[i * position + 1] = 2 * i + 3;
                triangles[i * position + 4] = (2 * i + 3) - 1;

                triangles[i * position + 2] = 2 * i + 1;
                triangles[i * position + 5] = (2 * i + 1) + 2;
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}