﻿using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab1
{
    public class Lab1Window : GameWindow
    {
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;

        public Lab1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 1 Hello, Triangle",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.ForestGreen);
            GL.Enable(EnableCap.CullFace);

            float[] vertices = new float[] { 0.0f, 0.8f,
 0.8f, 0.4f,
0.6f, -0.6f,
 -0.6f, -0.6f,
 -0.8f, 0.4f};
            uint[] indices = new uint[] { 0, 4, 3, 2, 1, 0 };

            /*  float[] vertices = new float[] { -0.4f, 0f,
              0.4f, 0f,
              0f, 0.6f,
              -0.8f, -0.6f,
              0f, -0.6f,
              0.8f, -0.6f,
              };

              uint[] indices = new uint[] {0,1,2,
              3,4,0,
              4,5,1};
            */
            GL.GenBuffers(2, mVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if(vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);

            if(indices.Length * sizeof(uint) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            /* GL.GenBuffers(1, out mVertexBufferObjectID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            */
            #region Shader Loading Code - Can be ignored for now

            mShader = new ShaderUtility( @"Lab1/Shaders/vSimple.vert", @"Lab1/Shaders/fSimple.frag");

            #endregion

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);

            // shader linking goes here
            #region Shader linking code - can be ignored for now

            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            #endregion

            GL.DrawElements(PrimitiveType.TriangleStrip, 6, DrawElementsType.UnsignedInt, 0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(1, mVertexBufferObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
