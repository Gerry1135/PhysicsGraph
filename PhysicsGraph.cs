using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace PhysicsGraph
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class PhysicsGraph : MonoBehaviour
    {
        private const int width = 500;
        private const int height = 100;
        private const int frameScale = 10;

        private Rect windowPos = new Rect(80, 80, 500, 200);
        private bool showUI = false;
        readonly Texture2D texRealTime = new Texture2D(width, height*2);
        private float[] realTimes = new float[width];
        
        readonly Texture2D texGameTime = new Texture2D(width, height);
        private float[] gameTimes = new float[width];

        readonly Texture2D texFrames = new Texture2D(width, height);
        private float[] frames = new float[width];

        int frameIndex = 0;
        int lastRendered = 0;

        long lastTime;

        long ticksPerMilliSec;

        long frameCount = 0;

        bool fullUpdate = false;

        Color[] blackLine;
        Color[] redLine;
        Color[] greenLine;
        Color[] blueLine;

		private GUIStyle labelStyle;
		private GUIStyle graphStyle;
		private GUILayoutOption wndWidth;
        private GUILayoutOption wndHeight;
		private GUILayoutOption graphWidth;
		private GUILayoutOption graphHeight;
		private GUILayoutOption graphHeight2;

        internal void Awake()
        {
            DontDestroyOnLoad(gameObject);

            redLine = new Color[height*2];
            greenLine = new Color[height*2];
            blueLine = new Color[height*2];
            blackLine = new Color[height*2];
            for (int i = 0; i < blackLine.Length; i++)
            {
                blackLine[i] = Color.black;
                redLine[i] = Color.red;
                greenLine[i] = Color.green;
                blueLine[i] = Color.blue;
            }

            for (int i = 0; i < width; i++)
            {
                realTimes[i] = 0;
                gameTimes[i] = 0;
                frames[i] = 0;
            }

            lastTime = Stopwatch.GetTimestamp();
            ticksPerMilliSec = Stopwatch.Frequency / 1000;
        }
        
        internal void OnDestroy()
        {
        }

        public void FixedUpdate()
        {
            // First thing is to record the times for this frame
            long time = Stopwatch.GetTimestamp();
            long timedelta = time - lastTime;
            realTimes[frameIndex] = timedelta / ticksPerMilliSec;
            lastTime = time;
            gameTimes[frameIndex] = (Time.fixedDeltaTime * 1000);
            frames[frameIndex] = frameCount;
            //print("real = " + realTimes[frameIndex] + "   game = " + gameTimes[frameIndex] + "   frames = " + frames[frameIndex]);
            frameIndex = (frameIndex + 1) % width;
            frameCount = 0;
        }

        public void Update()
        {
            //print("Update Start");
            
            frameCount++;

            if (GameSettings.MODIFIER_KEY.GetKey() && Input.GetKeyDown(KeyCode.Equals))
            {
                showUI = !showUI;
            }

            if (!showUI)
                return;

            if (fullUpdate)
            {
                fullUpdate = false;
                //lastRendered = (frameIndex - 1) % width;
            }

            // If we want to update this time
            if (lastRendered != frameIndex)
            {
                // Update the columns from lastRendered to frameIndex
                if (lastRendered >= frameIndex)
                {
                    for (int x = lastRendered; x < width; x++)
                    {
                        DrawColumn(texRealTime, x, (int)realTimes[x], redLine, height*2);
                        DrawColumn(texGameTime, x, (int)gameTimes[x], greenLine, height);
                        DrawColumn(texFrames, x, (int)(frames[x] * frameScale), blueLine, height);
                    }

                    lastRendered = 0;
                }
                    
                for (int x = lastRendered; x < frameIndex; x++)
                {
                    DrawColumn(texRealTime, x, (int)realTimes[x], redLine, height*2);
                    DrawColumn(texGameTime, x, (int)gameTimes[x], greenLine, height);
                    DrawColumn(texFrames, x, (int)(frames[x] * frameScale), blueLine, height);
                }

                lastRendered = frameIndex;

                if (frameIndex < width)
                    texRealTime.SetPixels(frameIndex, 0, 1, height*2, blackLine);
                if (frameIndex != width - 2)
                    texRealTime.SetPixels((frameIndex + 1) % width, 0, 1, height*2, blackLine);
                texRealTime.Apply();

                if (frameIndex < width)
                    texGameTime.SetPixels(frameIndex, 0, 1, height, blackLine);
                if (frameIndex != width - 2)
                    texGameTime.SetPixels((frameIndex + 1) % width, 0, 1, height, blackLine);
                texGameTime.Apply();

                if (frameIndex < width)
                    texFrames.SetPixels(frameIndex, 0, 1, height, blackLine);
                if (frameIndex != width - 2)
                    texFrames.SetPixels((frameIndex + 1) % width, 0, 1, height, blackLine);
                texFrames.Apply();
            }
            //print("Update End");
        }

        private void DrawColumn(Texture2D tex, int x, int y, Color[] col, int h)
        {
            //print("drawcol(" + x + ", " + y + ")");
            if (y > h - 1)
                y = h - 1;
            tex.SetPixels(x, 0, 1, y + 1, col);
            if (y < h - 1)
                tex.SetPixels(x, y + 1, 1, h - 1 - y, blackLine);
        }

        public void OnGUI()
        {
            if (labelStyle == null)
                labelStyle = new GUIStyle(GUI.skin.label);

            if (wndWidth == null)
                wndWidth = GUILayout.Width(width + 50);
            if (wndHeight == null)
                wndHeight = GUILayout.Height(height * 4 + 50);
			if (graphWidth == null)
				graphWidth = GUILayout.Width(width);
			if (graphHeight == null)
				graphHeight = GUILayout.Height(height);
			if (graphHeight2 == null)
				graphHeight2 = GUILayout.Height(height * 2);
			if (graphStyle == null)
				graphStyle = new GUIStyle();


            if (showUI)
                windowPos = GUILayout.Window(2461275, windowPos, WindowGUI, "PhysicsGraph" /*, wndWidth, wndHeight*/);
        }

        public void WindowGUI(int windowID)
        {
            GUILayout.BeginVertical();
			GUILayout.Box(texRealTime, graphStyle, graphWidth, graphHeight2);
			GUILayout.Box(texGameTime, graphStyle, graphWidth, graphHeight);
			GUILayout.Box(texFrames, graphStyle, graphWidth, graphHeight);
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}
