namespace FewClicksDev.ColorTextureGenerator
{
    using FewClicksDev.Core;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    using static FewClicksDev.Core.EditorDrawer;

    public class ColorTextureGeneratorWindow : CustomEditorWindow
    {
        public enum WindowMode
        {
            Gradient = 0,
            ColorRamp = 1
        }

        private const float TOOLBAR_WIDTH = 0.75f;
        private const float CREATE_BUTTON_WIDTH = 0.65f;
        private const float LABEL_WIDTH = 120f;
        private const float INDEX_WIDTH = 30f;
        private const float DELETE_COLOR_RAMP_WIDTH = 25f;
        private const float COVERAGE_WIDTH = 50f;
        private const float TOGGLE_WIDTH = 15f;
        private const int MIN_TEXTURE_SIZE = 16;

        private const string DEFAULT_PATH = "C:/";
        private const string GRADIENT_LABEL = "Gradient";
        private const string SETTINGS_LABEL = "Settings";
        private const string WIDTH_LABEL = "Width";
        private const string HEIGHT_LABEL = "Height";
        private const string FILE_NAME_LABEL = "File name";
        private const string INDEX_LABEL = "Index";
        private const string COLOR_LABEL = "Color";
        private const string COVERAGE_LABEL = "Coverage";
        private const string COLOR_LERP_LABEL = "Color lerp";
        private const string RESET_LIST_LABEL = "Reset list";
        private const string RESET_COVERAGE_LABEL = "Reset coverage";
        private const string PREVIEW_LABEL = "Preview";
        private const string EXPORT_FORMAT_LABEL = "Export format";
        private const string PIXELS_ORIENTATION_LABEL = "Pixels orientation";
        private const string DELETE = " X ";
        private const string PLUS = "+";
        private const string DOTS = "...";
        private const string FILE_NAME_CANNOT_BE_EMPTY = "File name cannot be empty.";
        private const string FILE_PATH = "File path";
        private const string FILE_PATH_ERROR = "The generated texture must be saved within the current project assets folder.";
        private const string OVERRIDE_FILE_AT_PATH = "Override file at path";

        private const string SAVE_GRADIENT_BUTTON_TEXT = "Save gradient to texture";
        private const string SAVE_COLOR_RAMP_BUTTON_TEXT = "Save color ramp to texture";
        private const string SAVE_WINDOW_TITLE_TEXT = "Select an output path for the texture";
        private const string PNG_EXTENSION = ".png";
        private const string TGA_EXTENSION = ".tga";
        private const string JPG_EXTENSION = ".jpg";

        protected override string windowName => "Color Texture Generator";
        protected override string version => ColorTextureGenerator.VERSION;
        protected override Vector2 minWindowSize => new Vector2(400f, 700f);
        protected override Color mainColor => ColorTextureGenerator.MAIN_COLOR;

        protected override bool hasDocumentation => true;
        protected override string documentationURL => "https://docs.google.com/document/d/1oQCWgvJgchAZtqUMHL0Hs6qGRhO96mrBfsd3dp2XO1c/edit?usp=sharing";
        protected override bool askForReview => true;
        protected override string reviewURL => "https://assetstore.unity.com/packages/slug/281099";

        private WindowMode windowMode = WindowMode.Gradient;
        private ExportFormat exportFormat = ExportFormat.PNG;

        private Gradient gradient = new Gradient();
        private Texture2D generatedGradientTexture = null;
        private int textureWidth = 128;
        private int textureHeight = 16;
        private PixelsOrientation pixelsOrientation = PixelsOrientation.Horizontal;
        private string fileName = "New Texture";
        private string filePath = DEFAULT_PATH;
        private bool overrideFileAtPath = false;

        private ColorRamp colorRamp = new ColorRamp();

        protected override void OnEnable()
        {
            base.OnEnable();

            if (filePath == DEFAULT_PATH)
            {
                filePath = Application.dataPath;
            }

            regenerateTextures();
        }

        protected override void drawWindowGUI()
        {
            NormalSpace();
            var _windowMode = this.DrawEnumToolbar(windowMode, TOOLBAR_WIDTH, mainColor);

            if (_windowMode != windowMode)
            {
                windowMode = _windowMode;
                regenerateTextures();
            }

            SmallSpace();
            DrawLine();
            SmallSpace();

            switch (windowMode)
            {
                case WindowMode.Gradient:
                    drawGradientTab();
                    break;

                case WindowMode.ColorRamp:
                    drawColorRampTab();
                    break;
            }
        }

        private void drawGradientTab()
        {
            SmallSpace();

            using (var _changeScope = new ChangeCheckScope())
            {
                using (new LabelWidthScope(LABEL_WIDTH))
                {
                    gradient = EditorGUILayout.GradientField(GRADIENT_LABEL, gradient);
                    drawTextureSettings();
                }

                if (_changeScope.changed)
                {
                    var _previewTextureSize = ColorTextureGenerator.GetPreviewTextureSize(textureWidth, textureHeight);
                    generatedGradientTexture = ColorTextureGenerator.CreateTextureFromGradient(gradient, pixelsOrientation, _previewTextureSize.Item1, _previewTextureSize.Item2);
                }
            }

            SmallSpace();
            DrawBoldLabel(PREVIEW_LABEL);
            drawTexturePreview(generatedGradientTexture);
            NormalSpace();

            if (fileName.IsNullEmptyOrWhitespace())
            {
                EditorGUILayout.HelpBox(FILE_NAME_CANNOT_BE_EMPTY, MessageType.Error);
                return;
            }

            using (new HorizontalScope())
            {
                FlexibleSpace();
                float _buttonWidth = windowWidthWithPaddings * CREATE_BUTTON_WIDTH;

                using (ColorScope.Background(BLUE))
                {
                    if (DrawBoxButton(SAVE_GRADIENT_BUTTON_TEXT, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                    {
                        Texture2D _texture = ColorTextureGenerator.CreateTextureFromGradient(gradient, pixelsOrientation, textureWidth, textureHeight);
                        saveAndPingTheTexture(filePath, _texture, fileName);
                    }
                }

                FlexibleSpace();
            }
        }

        private void drawColorRampTab()
        {
            SmallSpace();

            using (new HorizontalScope())
            {
                GUILayout.Label(INDEX_LABEL, EditorStyles.centeredGreyMiniLabel, FixedWidth(INDEX_WIDTH));
                GUILayout.Label(COLOR_LABEL, EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label(COVERAGE_LABEL, EditorStyles.centeredGreyMiniLabel, FixedWidth(COVERAGE_WIDTH));
                GUILayout.Label(COLOR_LERP_LABEL, EditorStyles.centeredGreyMiniLabel, FixedWidth(TOGGLE_WIDTH + COVERAGE_WIDTH));
                GUILayout.Label(string.Empty, FixedWidth(DELETE_COLOR_RAMP_WIDTH));
            }

            using (new LabelWidthScope(LABEL_WIDTH))
            {
                int _index = 0;

                using (var _changeScope = new ChangeCheckScope())
                {
                    foreach (var _color in colorRamp.Colors)
                    {
                        using (new HorizontalScope(Styles.BoxButton, FixedHeight(DEFAULT_LINE_HEIGHT)))
                        {
                            GUIStyle _label = new GUIStyle(EditorStyles.label);
                            _label.alignment = TextAnchor.MiddleLeft;

                            GUILayout.Label($" {(_index + 1).NumberToString(2)}", _label, FixedWidth(INDEX_WIDTH));
                            _color.RampColor = EditorGUILayout.ColorField(_color.RampColor);
                            _color.Coverage = EditorGUILayout.FloatField(_color.Coverage, FixedWidth(COVERAGE_WIDTH));

                            if (_index == colorRamp.NumberOfColors - 1)
                            {
                                _color.UseLerpToNextColor = false;
                                GUILayout.Label(string.Empty, FixedWidth(TOGGLE_WIDTH + COVERAGE_WIDTH + 1f)); // Empty space when it's the last color
                            }
                            else
                            {
                                _color.UseLerpToNextColor = EditorGUILayout.Toggle(_color.UseLerpToNextColor, FixedWidth(TOGGLE_WIDTH));

                                using (new DisabledScope(_color.UseLerpToNextColor == false))
                                {
                                    _color.LerpCoverage = EditorGUILayout.FloatField(_color.LerpCoverage, FixedWidth(COVERAGE_WIDTH));
                                }
                            }

                            if (DrawBoxButton(DELETE, FixedWidthAndHeight(DELETE_COLOR_RAMP_WIDTH, DEFAULT_LINE_HEIGHT)))
                            {
                                colorRamp.Colors.RemoveAt(_index);
                                break;
                            }
                        }

                        _index++;
                    }

                    SmallSpace();

                    using (new HorizontalScope())
                    {
                        if (DrawBoxButton(RESET_LIST_LABEL, FixedWidthAndHeight(75f, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRamp.ResetList();
                        }

                        SmallSpace();

                        if (DrawBoxButton(RESET_COVERAGE_LABEL, FixedWidthAndHeight(105f, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRamp.ResetCoverage();
                        }

                        FlexibleSpace();

                        if (colorRamp.CanAddColors && DrawBoxButton(PLUS, FixedWidthAndHeight(25f, DEFAULT_LINE_HEIGHT)))
                        {
                            colorRamp.AddColor(Color.white, 1f);
                        }
                    }

                    drawTextureSettings();

                    if (_changeScope.changed)
                    {
                        colorRamp.RegenerateTexture(pixelsOrientation, textureWidth, textureHeight);
                    }
                }
            }

            SmallSpace();
            DrawBoldLabel(PREVIEW_LABEL);
            drawTexturePreview(colorRamp.GeneratedTexture);
            NormalSpace();

            if (fileName.IsNullEmptyOrWhitespace())
            {
                EditorGUILayout.HelpBox(FILE_NAME_CANNOT_BE_EMPTY, MessageType.Error);
                return;
            }

            using (new HorizontalScope())
            {
                FlexibleSpace();
                float _buttonWidth = windowWidthWithPaddings * CREATE_BUTTON_WIDTH;

                using (ColorScope.Background(BLUE))
                {
                    if (DrawBoxButton(SAVE_COLOR_RAMP_BUTTON_TEXT, FixedWidthAndHeight(_buttonWidth, DEFAULT_LINE_HEIGHT)))
                    {
                        Texture2D _texture = ColorTextureGenerator.CreateTextureFromColorRamp(colorRamp, pixelsOrientation, textureWidth, textureHeight);
                        saveAndPingTheTexture(filePath, _texture, fileName);
                    }
                }

                FlexibleSpace();
            }
        }

        private void drawTexturePreview(Texture2D _texture)
        {
            GUIStyle _textureStyle = new GUIStyle();
            _textureStyle.normal.background = _texture;

            using (new HorizontalScope())
            {
                FlexibleSpace();
                EditorGUILayout.LabelField(string.Empty, _textureStyle, FixedWidthAndHeight(_texture.width, _texture.height));
                FlexibleSpace();
            }
        }

        private void saveAndPingTheTexture(string _folderPath, Texture2D _texture, string _fileName)
        {
            string _filePath = $"{_folderPath}/{_fileName}{getFileExtension()}";
            _filePath = AssetsUtilities.ConvertAbsolutePathToDataPath(_filePath);

            if (overrideFileAtPath == false)
            {
                _filePath = AssetDatabase.GenerateUniqueAssetPath(_filePath);
            }

            byte[] _pixelsData = null;

            switch (exportFormat)
            {
                case ExportFormat.PNG:
                    _pixelsData = _texture.EncodeToPNG();
                    break;

                case ExportFormat.TGA:
                    _pixelsData = _texture.EncodeToTGA();
                    break;

                case ExportFormat.JPG:
                    _pixelsData = _texture.EncodeToJPG();
                    break;
            }

            File.WriteAllBytes(_filePath, _pixelsData);
            AssetDatabase.Refresh();

            Texture2D _loaded = AssetDatabase.LoadAssetAtPath<Texture2D>(_filePath);
            AssetsUtilities.Ping(_loaded);
        }

        private string getFileExtension()
        {
            return exportFormat switch
            {
                ExportFormat.PNG => PNG_EXTENSION,
                ExportFormat.TGA => TGA_EXTENSION,
                ExportFormat.JPG => JPG_EXTENSION,
                _ => PNG_EXTENSION,
            };
        }

        private void drawTextureSettings()
        {
            NormalSpace();
            DrawBoldLabel(SETTINGS_LABEL);
            textureWidth = Mathf.Max(MIN_TEXTURE_SIZE, EditorGUILayout.IntField(WIDTH_LABEL, textureWidth));
            textureHeight = Mathf.Max(MIN_TEXTURE_SIZE, EditorGUILayout.IntField(HEIGHT_LABEL, textureHeight));

            SmallSpace();
            pixelsOrientation = (PixelsOrientation) EditorGUILayout.EnumPopup(PIXELS_ORIENTATION_LABEL, pixelsOrientation);
            exportFormat = (ExportFormat) EditorGUILayout.EnumPopup(EXPORT_FORMAT_LABEL, exportFormat);

            SmallSpace();
            fileName = EditorGUILayout.TextField(FILE_NAME_LABEL, fileName);

            using (new HorizontalScope())
            {
                using (new DisabledScope())
                {
                    EditorGUILayout.TextField(FILE_PATH, filePath);
                }

                if (GUILayout.Button(DOTS, FixedWidth(20f)))
                {
                    string _filePath = EditorUtility.SaveFolderPanel(SAVE_WINDOW_TITLE_TEXT, Application.dataPath, string.Empty);

                    if (_filePath.StartsWith(Application.dataPath) == false)
                    {
                        ColorTextureGenerator.Error(FILE_PATH_ERROR);
                    }
                    else
                    {
                        filePath = _filePath;
                    }
                }
            }

            overrideFileAtPath = EditorGUILayout.Toggle(OVERRIDE_FILE_AT_PATH, overrideFileAtPath);
            NormalSpace();
        }

        private void regenerateTextures()
        {
            colorRamp.RegenerateTexture(pixelsOrientation, textureWidth, textureHeight);

            var _previewTextureSize = ColorTextureGenerator.GetPreviewTextureSize(textureWidth, textureHeight);
            generatedGradientTexture = ColorTextureGenerator.CreateTextureFromGradient(gradient, pixelsOrientation, _previewTextureSize.Item1, _previewTextureSize.Item2);
        }

        [MenuItem("Window/FewClicks Dev/Color Texture Generator")]
        private static void ShowWindow()
        {
            GetWindow<ColorTextureGeneratorWindow>().Show();
        }
    }
}