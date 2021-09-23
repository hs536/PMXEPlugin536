using PEPlugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PMXEPlugin536.MaskImageToEdge {
    public class Runner : PEPluginClass {

        public Runner() : base() {
            var runOnStartup = false;
            var addPluginMenu = true;
            var pluginName = "Apply Mask Image To Edge";
            base.m_option = new PEPluginOption(runOnStartup, addPluginMenu, pluginName);
        }

        public override void Run(IPERunArgs args) {
            try {
                var pmx = args.Host.Connector.Pmx;

                // make sure a material is selected
                int materialIndex = args.Host.Connector.Form.SelectedMaterialIndex;
                if (materialIndex == -1) {
                    MessageBox.Show("Please select a material.");
                    return;
                }

                // create candidate vertex list
                var vertexSet = new HashSet<ComparableVertex>();
                var faces = args.Host.Connector.Pmx.GetCurrentState().Material[materialIndex].Faces;
                foreach (var face in faces) {
                    vertexSet.Add(new ComparableVertex(face.Vertex1.Position.X, face.Vertex1.Position.Y, face.Vertex1.Position.Z));
                    vertexSet.Add(new ComparableVertex(face.Vertex2.Position.X, face.Vertex2.Position.Y, face.Vertex2.Position.Z));
                    vertexSet.Add(new ComparableVertex(face.Vertex3.Position.X, face.Vertex3.Position.Y, face.Vertex3.Position.Z));
                }

                // backup model file
                var modelFilePath = pmx.CurrentPath;
                if (modelFilePath.Length != 0) {
                    backupModelFile(modelFilePath);
                }

                // load image
                var bitmap = getImageBitmap();
                if (bitmap == null) {
                    MessageBox.Show("Please select image file.");
                    return;
                }

                // overwrite vertex
                var currentPmxData = pmx.GetCurrentState();
                var vertexList = currentPmxData.Vertex;
                foreach (var vertex in vertexList) {
                    var comparableVertex = new ComparableVertex(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
                    if (!vertexSet.Contains(comparableVertex)) {
                        continue;
                    }
                    var uv = vertex.UV;
                    int x = (int)Math.Round(uv.U * (bitmap.Width - 1), 0);
                    int y = (int)Math.Round(uv.V * (bitmap.Height - 1), 0);
                    var pixel = bitmap.GetPixel(x, y);
                    vertex.EdgeScale = convertRGBtoLuma(pixel.R, pixel.G, pixel.B);
                }

                // Update Pmx and View
                pmx.Update(currentPmxData);
                args.Host.Connector.View.PmxView.UpdateModel();

                MessageBox.Show("done.");
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static void backupModelFile(String filepath,
            String suffix = ".bak", bool overwrite = true) {
            File.Copy(filepath, filepath + suffix, overwrite);
        }

        private static Bitmap getImageBitmap() {
            var dialog = new OpenFileDialog();
            dialog.Filter = @"Image File(*.bmp,*.jpg,*.png,*.tif)|*.bmp;*.jpg;*.png;*.tif";
            if (dialog.ShowDialog() == DialogResult.Cancel) {
                return null;
            }
            var fileNameWithPath = dialog.FileName;
            var extension = Path.GetExtension(fileNameWithPath).ToLower();
            if (@".bmp|.jpg|.png|.tif".IndexOf(extension) == -1) {
                return null;
            }

            Bitmap bmp;
            using (var stream = new FileStream(fileNameWithPath, FileMode.Open, FileAccess.Read)) {
                bmp = new Bitmap(stream);
            }
            return bmp;
        }

        private static float convertRGBtoLuma(int red, int green, int blue) {
            // calc with CIE coefficient
            var calculated_val = (0.2126 * red + 0.7152 * green + 0.0722 * blue) / 255;
            var simplified_val = (calculated_val < 0.01) ? 0.0f
                : (calculated_val > 0.995) ? 1.0f : (float)Math.Round(calculated_val, 2);
            return simplified_val;
        }

        private class ComparableVertex {
            float X { get; set; }
            float Y { get; set; }
            float Z { get; set; }

            public ComparableVertex(float X, float Y, float Z) {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
            }

            public override bool Equals(object obj) {
                ComparableVertex target = (ComparableVertex)obj;
                return (this.X == target.X && this.Y == target.Y && this.Z == target.Z);
            }

            public override int GetHashCode() {
                return 0;
            }
        }
    }
}
