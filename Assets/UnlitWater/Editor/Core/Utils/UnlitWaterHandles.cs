using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ASL.UnlitWater
{
    internal static class UnlitWaterHandles
    {
        public static void DrawUnlitWaterArea(Vector3 position, Quaternion rotation, Vector2 size, Vector2 heightRange,
            Color color)
        {

            Vector3 pos1 = position + rotation*new Vector3(size.x, 0, size.y);
            Vector3 pos2 = position + rotation*new Vector3(size.x, 0, -size.y);
            Vector3 pos3 = position + rotation*new Vector3(-size.x, 0, -size.y);
            Vector3 pos4 = position + rotation*new Vector3(-size.x, 0, size.y);

            Vector3 pos5 = position + rotation*new Vector3(size.x, -heightRange.x, size.y);
            Vector3 pos6 = position + rotation*new Vector3(size.x, -heightRange.x, -size.y);
            Vector3 pos7 = position + rotation*new Vector3(-size.x, -heightRange.x, -size.y);
            Vector3 pos8 = position + rotation*new Vector3(-size.x, -heightRange.x, size.y);

            Vector3 pos9 = position + rotation*new Vector3(size.x, heightRange.y, size.y);
            Vector3 pos10 = position + rotation*new Vector3(size.x, heightRange.y, -size.y);
            Vector3 pos11 = position + rotation*new Vector3(-size.x, heightRange.y, -size.y);
            Vector3 pos12 = position + rotation*new Vector3(-size.x, heightRange.y, size.y);

            Handles.color = color;

            Handles.DrawLine(pos1, pos2);
            Handles.DrawLine(pos2, pos3);
            Handles.DrawLine(pos3, pos4);
            Handles.DrawLine(pos4, pos1);

            Handles.DrawLine(pos5, pos6);
            Handles.DrawLine(pos6, pos7);
            Handles.DrawLine(pos7, pos8);
            Handles.DrawLine(pos8, pos5);

            Handles.DrawLine(pos9, pos10);
            Handles.DrawLine(pos10, pos11);
            Handles.DrawLine(pos11, pos12);
            Handles.DrawLine(pos12, pos9);

            Handles.DrawLine(pos9, pos5);
            Handles.DrawLine(pos10, pos6);
            Handles.DrawLine(pos11, pos7);
            Handles.DrawLine(pos12, pos8);
        }

        public static void DrawUnlitWaterLodCells(Vector3 position, Quaternion rotation, Vector2 size, int cellSizeX,
            int cellSizeZ, int maxLod)
        {
            float deltax = size.x*2/cellSizeX;
            float deltaz = size.y*2/cellSizeZ;

            int dt = (int) Mathf.Pow(2, maxLod);
            float loddeltax = deltax/dt;
            float loddeltaz = deltaz/dt;

            for (int i = 0; i < cellSizeX; i++)
            {
                if (i > 0)
                {
                    Handles.color = Color.blue;
                    Vector3 posb = position + rotation*new Vector3(-size.x + i*deltax, 0, size.y);
                    Vector3 posf = position + rotation*new Vector3(-size.x + i*deltax, 0, -size.y);
                    Handles.DrawLine(posb, posf);
                }
                Handles.color = Color.yellow;
                for (int j = 1; j < dt; j++)
                {
                    Vector3 posb = position + rotation*new Vector3(-size.x + i*deltax + j*loddeltax, 0, size.y);
                    Vector3 posf = position + rotation*new Vector3(-size.x + i*deltax + j*loddeltax, 0, -size.y);
                    Handles.DrawLine(posb, posf);
                }
            }

            for (int i = 0; i < cellSizeZ; i++)
            {
                if (i > 0)
                {
                    Handles.color = Color.blue;
                    Vector3 posb = position + rotation*new Vector3(-size.x, 0, -size.y + i*deltaz);
                    Vector3 posf = position + rotation*new Vector3(size.x, 0, -size.y + i*deltaz);
                    Handles.DrawLine(posb, posf);
                }
                Handles.color = Color.yellow;
                for (int j = 1; j < dt; j++)
                {
                    Vector3 posb = position + rotation*new Vector3(-size.x, 0, -size.y + i*deltaz + j*loddeltaz);
                    Vector3 posf = position + rotation*new Vector3(size.x, 0, -size.y + i*deltaz + j*loddeltaz);
                    Handles.DrawLine(posb, posf);
                }
            }
        }

        public static void DrawUnlitWaterGrid(Vector3 position, Quaternion rotation, Vector2 size, int cellSizeX,
            int cellSizeZ)
        {
            float deltax = size.x*2/cellSizeX;
            float deltaz = size.y*2/cellSizeZ;

            for (int i = 0; i <= cellSizeX; i++)
            {
                Handles.color = Color.yellow;
                Vector3 posb = position + rotation * new Vector3(-size.x+i*deltax, 0, -size.y);
                Vector3 posf = position + rotation * new Vector3(-size.x+i*deltax, 0, size.y);
                Handles.DrawLine(posb, posf);
            }

            for (int i = 0; i <= cellSizeZ; i++)
            {
                Handles.color = Color.yellow;
                Vector3 posb = position + rotation * new Vector3(-size.x, 0, -size.y + i * deltaz);
                Vector3 posf = position + rotation * new Vector3(size.x, 0, -size.y+i*deltaz);
                Handles.DrawLine(posb, posf);
            }
        }

        public static void DrawDirArrow(Vector3 position, float angle, float size, Color color)
        {
            Handles.color = color;
#if UNITY_5_5_OR_NEWER
            Handles.ArrowHandleCap(0, position, Quaternion.Euler(0, angle + 90, 0), size, EventType.Repaint);
#else
            Handles.ArrowCap(0, position, Quaternion.Euler(0, angle + 90, 0), size);
#endif
        }
    }
}