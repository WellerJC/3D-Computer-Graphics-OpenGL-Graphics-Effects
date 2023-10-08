using Labs.Utility;
using Microsoft.SqlServer.Server;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Markup;

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[20];
        private int[] mVAO_IDs = new int[15];
        private int[] mTexture_ID = new int[5];
        private ShaderUtility mShader;
        private ModelUtility mSphereModelUtility, mCustomModelUtility, mPodiumUtility, mCubeModelUtility;
        private Matrix4 mView, mCustomModel, mSphereModel, mCubeModel, mGroundModel, mPodium1, mPodium2, mPodium3, mWall1, mWall2, mWall3, mMoon, mRoof;
        private Vector4 lightDirection, eyePosition;
        Vector3 colour; 
        
        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.MintCream); GL.Enable(EnableCap.DepthTest); GL.Enable(EnableCap.CullFace); GL.Enable(EnableCap.Texture2D);
            mShader = new ShaderUtility(@"ACW/Shaders/vACW.vert", @"ACW/Shaders/fACW.frag");  GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition"); int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");

            mView = Matrix4.CreateTranslation(0, -1.5f, 0); int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            lightDirection = Vector4.Transform(new Vector4(0, 20, 0, 20), mView); int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightDirection");
            GL.Uniform4(uLightDirectionLocation, lightDirection);


            colour = new Vector3(0f, 0f, 0f);  int uAmbientDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uAmbientLight");
            GL.Uniform3(uAmbientDirectionLocation, colour);

            eyePosition = Vector4.Transform(new Vector4(2, 5, -10.5f, 1), mView); int uEyePosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");
            GL.Uniform4(uEyePosition, eyePosition);  GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs); GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);


            #region Textures 

            float[] texCoords = {-10, 0, -10,1,1,1,
                                                  -10, 0, 10,1,1,1,
                                                    10, 0, 10,1,1,1,
                                                   10, 0, -10,1,1,1};

                             GL.Enable(EnableCap.Texture2D);
                             string filepath = @"Lab4/brick.jpg";
                             if (System.IO.File.Exists(filepath))
                             {
                                 Bitmap TextureBitmap = new Bitmap(filepath);
                                 BitmapData TextureData = TextureBitmap.LockBits(new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height),
                                     ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                 GL.ActiveTexture(TextureUnit.Texture0);
                                 GL.GenTextures(1, out mTexture_ID[1]);
                                 GL.BindTexture(TextureTarget.Texture2D, mTexture_ID[1]);
                                 GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
                                 GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                                 GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                                 TextureBitmap.UnlockBits(TextureData);
                             }
                             else
                             {
                                 throw new Exception("Could not find file " + filepath);
                             }
                             int vTexCoords = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");
                             int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler");
                             GL.Uniform1(uTextureSamplerLocation, 0);
                             GL.BindVertexArray(mTexture_ID[1]);
                             GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[9]);
                             GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texCoords.Length * sizeof(float)), texCoords, BufferUsageHint.StaticDraw);
                             GL.EnableVertexAttribArray(vTexCoords);
                             GL.VertexAttribPointer(vTexCoords, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);





            #endregion

            #region Floor
           

            float[] FloorVertices = new float[] {-10, 0, -10,0,1,0,
                                                              -10, 0, 10,0,1,0,
                                                              10, 0, 10,0,1,0,
                                                              10, 0, -10,0,1,0,};           
                             DrawPlane(FloorVertices, 0, 0, vPositionLocation, vNormalLocation);
            

            float[] RoofVertices = new float[] {10, 10, -10,0,1,0,
                                             10, 10, 10,0,1,0,
                                             -10, 10, 10,0,1,0,
                                             -10, 10, -10,0,1,0,};
            DrawPlane(RoofVertices, 9, 12, vPositionLocation, vNormalLocation);
            #endregion

            #region Walls
         
            float[] FrontWallVertices = new float[] {10, -10, -10,0,1,0,
                                             10, -10, 10,0,1,0,
                                             10, 10, 10,0,1,0,
                                             10, 10, -10,0,1,0,};
            DrawPlane(FrontWallVertices, 5, 7, vPositionLocation, vNormalLocation);
            
            float[] LeftWallVertices = new float[] {-10, 10, -10,0,1,0,
                                             -10, 10, 10,0,1,0,
                                             -10, -10, 10,0,1,0,
                                             -10, -10, -10,0,1,0,};
            DrawPlane(LeftWallVertices, 6, 8, vPositionLocation, vNormalLocation);  
            
            float[] RightWallVertices = new float[] {10, -10, -10,0,1,0,
                                             10, 10, -10,0,1,0,
                                             -10, 10, -10,0,1,0,
                                             -10, -10, -10,0,1,0,};
            DrawPlane(RightWallVertices, 7, 9, vPositionLocation, vNormalLocation);
            
            #endregion

            #region Sphere
            mSphereModelUtility = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");
            LoadModel(mSphereModelUtility, 1, 1, 2, vPositionLocation, vNormalLocation, vTexCoords, @"Lab4/brick.jpg", 0, TextureUnit.Texture1);           
            #endregion

            #region Model
            mCustomModelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");
            LoadModel(mCustomModelUtility, 2, 3, 4, vPositionLocation, vNormalLocation, vTexCoords, @"Lab4/texture.jpg", 4, TextureUnit.Texture2);            
            #endregion

            #region Cube
            mCubeModelUtility = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");
           
            LoadModel(mCubeModelUtility, 8, 10, 11, vPositionLocation, vNormalLocation, vTexCoords, @"Lab4/texture.jpg",1, TextureUnit.Texture3);
            #endregion

            #region Podiums
            mPodiumUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
            LoadModel(mPodiumUtility, 3, 5, 6, vPositionLocation, vNormalLocation,vTexCoords, @"Lab4/brick.jpg", 0, TextureUnit.Texture1);
            #endregion

            #region Positions         
            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f); mRoof = Matrix4.CreateTranslation(0, 3, -5f);
            mWall1 = Matrix4.CreateTranslation(0, 3, -5f); mWall2 = Matrix4.CreateTranslation(0, 3, -5f); mWall3 = Matrix4.CreateTranslation(0, 3, -5f);
            mSphereModel = Matrix4.CreateTranslation(3, 3, -3f); mMoon = Matrix4.CreateTranslation(25, 20, -15);
            mCustomModel = Matrix4.CreateTranslation(0, 3, -6f); mCubeModel = Matrix4.CreateTranslation(-1f, 1f, -1f);
            mPodium1 = Matrix4.CreateTranslation(0, 0.8f, -6f); mPodium2 = Matrix4.CreateTranslation(3, 0.8f, -3f); mPodium3 = Matrix4.CreateTranslation(-3, 0.8f, -3f);

            Vector3 v = mCustomModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(v); Matrix4 inverseTranslation = Matrix4.CreateTranslation(-v);
            mCustomModel = mCustomModel * inverseTranslation * Matrix4.CreateRotationY(5f) * translation;
            mMoon = mMoon * Matrix4.CreateScale(0.2f, 0.2f, 0.2f); mCubeModel = mCubeModel * Matrix4.CreateScale(3, 3, 3);
            #endregion

            base.OnLoad(e);

        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            switch(e.KeyChar){
                case 'w': Movement(0.05f, 0, 0); break; case 'W': Movement(0.05f, 0, 0); break; // Forward
                case 's': Movement(-0.05f, 0, 0); break; case 'S': Movement(-0.05f, 0, 0); break; // Back
                case 'a': Movement(0, 0, -0.025f); break; case 'A': Movement(0, 0, -0.025f); break; // Left
                case 'd': Movement(0, 0, 0.025f); break; case 'D': Movement(0, 0, 0.025f); break; // Right
                case 'q': Movement(0, -0.05f, 0); break; case 'Q': Movement(0, -0.05f, 0); break; // Up
                case 'e': Movement(0, 0.05f, 0); break; case 'E': Movement(0, 0.05f, 0); break; // Down

                case 'z': Transformations("Walls", -1, 1, -2); break; // Move Walls
                case 'Z': Transformations("Walls", -1, 1, -2); break;
                case 'x': Transformations("Walls", 1, -1, 2); break;
                case 'X': Transformations("Walls", 1, -1, 2); break;

                case 'c': Transformations("Model", 0.99f, 0, 0); break; // Scale Down
                case 'C': Transformations("Model", 0.99f, 0, 0); break;
                case 'v': Transformations("Model", 1.01f, 0, 0); break; // Scale Up
                case 'V': Transformations("Model", 1.01f, 0, 0); break;

                case '1': LightPosition(15, 20, 15); break; // Light Pos 1
                case '2': LightPosition(15, 20, -15); break; // Light Pos 2
                case '3': LightPosition(-15, 20, -15); break; // Light Pos 3
                case '4': LightPosition(-15, 20, 15); break; // Light Pos 4

                case '5': CameraAngle(0, -2.5f, 0, 0); break; // Camera Angle 1
                case '6': CameraAngle(9.5f, -6f, 0, 0.8f); break; // Camera Angle 2
                case '7': CameraAngle(-9.5f, -6f, 0, -0.8f); break; // Camera Angle 3
                case '8': CameraAngle(0, -3, -9.5f, 0); break; // Camera Angle 4
            }         
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
           
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);      

            #region Scene
            Scene(mGroundModel, 0);  Scene(mRoof, 9);

            Scene(mWall1, 5); Scene(mWall2, 6); Scene(mWall3, 7);
            #endregion

            #region Models
            Models(mSphereModel, mSphereModelUtility, 1);
            Vector3 v = mSphereModel.ExtractTranslation(); Matrix4 translation = Matrix4.CreateTranslation(v); Matrix4 inverseTranslation = Matrix4.CreateTranslation(-v);
            mSphereModel = mSphereModel * inverseTranslation * Matrix4.CreateRotationY(-0.015f) * translation;

            Models(mCustomModel, mCustomModelUtility, 2);

            Models(mCubeModel, mCubeModelUtility, 8);
            Vector3 v1 = mCubeModel.ExtractTranslation();  Matrix4 translation2 = Matrix4.CreateTranslation(v1); Matrix4 inverseTranslation2 = Matrix4.CreateTranslation(-v1);
            mCubeModel = mCubeModel * inverseTranslation2 * Matrix4.CreateRotationY(-0.05f) * translation2; mCubeModel = mCubeModel * inverseTranslation2 * Matrix4.CreateRotationX(0.025f) * translation2;

            Models(mMoon, mSphereModelUtility, 1);         
            mMoon = mMoon * inverseTranslation * Matrix4.CreateRotationY(0.015f) * translation; mMoon = mMoon * inverseTranslation * Matrix4.CreateRotationZ(0.015f) * translation;
            #endregion

            #region Podiums
            Models(mPodium1, mPodiumUtility, 3); Models(mPodium2, mPodiumUtility, 3); Models(mPodium3, mPodiumUtility, 3);
            #endregion

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }
        
        private void DrawPlane(float[] vertices, int a, int b, int position, int normal)
        {
            GL.BindVertexArray(mVAO_IDs[a]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[b]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            GL.EnableVertexAttribArray(position);
            GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(normal);
            GL.VertexAttribPointer(normal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
        }

        private void LoadModel(ModelUtility utility, int a, int b1, int b2, int position, int normal, int textureCoord, string filename, int id, TextureUnit unit)
        {
            int size;
            GL.BindVertexArray(mVAO_IDs[a]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[b1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(utility.Vertices.Length * sizeof(float)), utility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[b2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(utility.Indices.Length * sizeof(float)), utility.Indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (utility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (utility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(textureCoord);
            GL.VertexAttribPointer(textureCoord, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            // Set position and normal attributes
            GL.EnableVertexAttribArray(position);
            GL.VertexAttribPointer(position, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(normal);
            GL.VertexAttribPointer(normal, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
        }

        private void Models(Matrix4 model, ModelUtility utility, int i)
        {
            GL.Color3(1.0f, 0.0f, 0.0f);
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            Matrix4 m = model * mGroundModel;
            GL.UniformMatrix4(uModel, true, ref m);
            GL.BindVertexArray(mVAO_IDs[i]);
            GL.DrawElements(PrimitiveType.Triangles, utility.Indices.Length, DrawElementsType.UnsignedInt, 0);  
        }

        private void Scene(Matrix4 model, int i)
        {
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref model);
            GL.BindVertexArray(mVAO_IDs[i]);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
        }

        private void Movement(float motion, float TUD, float TLR)
        {
            mView = mView * Matrix4.CreateTranslation(0.0f, 0.0f, motion);
            mView = mView * Matrix4.CreateRotationY(TLR);
            mView = mView * Matrix4.CreateRotationX(TUD);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }

        private void Transformations(string transform, float value, float value2, float value3)
        {
            if (transform == "Walls")
            {
                Vector3 t = mGroundModel.ExtractTranslation();
                Matrix4 translation = Matrix4.CreateTranslation(t);
                Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
                mWall1 = mWall1 * inverseTranslation * Matrix4.CreateRotationY(value) * translation;
                mWall2 = mWall2 * inverseTranslation * Matrix4.CreateRotationY(value2) * translation;
                mWall3 = mWall3 * inverseTranslation * Matrix4.CreateRotationY(value3) * translation;
            }
            else if (transform == "Model")
            {
                mCustomModel = mCustomModel * Matrix4.CreateScale(value, value, value);
            }
        }

        private void LightPosition(float value1, float value2, float value3)
        {
            lightDirection = Vector4.Transform(new Vector4(value1, value2, value3, 20), mView);
            int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightDirection");
            GL.Uniform4(uLightDirectionLocation, lightDirection);
        }

        private void CameraAngle(float x, float y, float z, float rotate)
        {
            mView = Matrix4.CreateTranslation(x, y, z);
            mView = mView * Matrix4.CreateRotationY(rotate);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }
    }
}
