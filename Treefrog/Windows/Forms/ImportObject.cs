﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Aux;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Forms
{
    public partial class ImportObject : Form
    {
        private ValidationController _validateController;

        public ImportObject ()
        {
            InitializeForm();

            _validateController.Validate();
        }

        private void InitializeForm ()
        {
            InitializeComponent();

            _validateController = new ValidationController() {
                OKButton = _buttonOK,
            };

            _validateController.RegisterControl(_textObjectName, ValidateObjectName);
            _validateController.RegisterControl(_textSource, ValidateSourceFile);
            _validateController.RegisterControl(_numMaskLeft,
                ValidationController.ValidateLessEq("Left Mask Bound", _numMaskLeft, "Right Mask Bound", _numMaskRight));
            _validateController.RegisterControl(_numMaskRight,
                ValidationController.ValidateGreaterEq("Right Mask Bound", _numMaskRight, "Left Mask Bound", _numMaskLeft));
            _validateController.RegisterControl(_numMaskTop,
                ValidationController.ValidateLessEq("Top Mask Bound", _numMaskTop, "Bottom Mask Bound", _numMaskBottom));
            _validateController.RegisterControl(_numMaskBottom,
                ValidationController.ValidateGreaterEq("Bottom Mask Bound", _numMaskBottom, "Top Mask Bound", _numMaskTop));
            _validateController.RegisterControl(_numOriginX,
                ValidationController.ValidateNumericUpDownFunc("Origin X", _numOriginX));
            _validateController.RegisterControl(_numOriginY,
                ValidationController.ValidateNumericUpDownFunc("Origin Y", _numOriginY));
        }

        protected override void OnLoad(EventArgs e)
        {
 	         base.OnLoad(e);
            _textObjectName.Text = FindDefaultName("Object");
        }

        #region Object Name

        #region Reserved Names

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        #endregion

        private string _name = "";

        public string ObjectName
        {
            get { return _name; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_name != value) {
                    _name = value;
                }
            }
        }

        #endregion

        #region Source File

        private string _sourceFile = "";

        public string SourceFile
        {
            get { return _sourceFile; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_sourceFile != value) {
                    _sourceFile = value;
                }
            }
        }

        private void OpenSourceFile ()
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "Image Files (*.bmp,*.gif,*.jpg,*.png)|*.bmp;*.gif;*.jpg;*.jpeg;*.png|All files (*.*)|*.*";
            dlg.FilterIndex = 0;
            dlg.RestoreDirectory = true;
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                LoadObjectPreview(dlg.FileName);
                SourceFile = dlg.FileName;
                _textSource.Text = dlg.FileName;
            }
        }

        #endregion

        #region Object Parameters

        private int? _maskLeft = 0;
        private int? _maskTop = 0;
        private int? _maskRight = 0;
        private int? _maskBottom = 0;
        private int? _originX = 0;
        private int? _originY = 0;

        public int? MaskLeft
        {
            get { return _maskLeft; }
            set
            {
                if (_maskLeft != value) {
                    _maskLeft = value;
                }
            }
        }

        public int? MaskTop
        {
            get { return _maskTop; }
            set
            {
                if (_maskTop != value) {
                    _maskTop = value;
                }
            }
        }

        public int? MaskRight
        {
            get { return _maskRight; }
            set
            {
                if (_maskRight != value) {
                    _maskRight = value;
                }
            }
        }

        public int? MaskBottom
        {
            get { return _maskBottom; }
            set
            {
                if (_maskBottom != value) {
                    _maskBottom = value;
                }
            }
        }

        public int? OriginX
        {
            get { return _originX; }
            set
            {
                if (_originX != value) {
                    UpdateOrigin(value, _originY);
                }
            }
        }

        public int? OriginY
        {
            get { return _originY; }
            set
            {
                if (_originY != value) {
                }
            }
        }

        private void UpdateOrigin (int? x, int? y)
        {
            int oldX = _originX ?? 0;
            int oldY = _originY ?? 0;

            int diffX = (x ?? 0) - oldX;
            int diffY = (y ?? 0) - oldY;

            _originX = x;
            _originY = y;
            _maskLeft -= diffX;
            _maskTop -= diffY;
            _maskRight -= diffX;
            _maskBottom -= diffY;

            UpdateMaskPropertyFields();
            //RaiseMaskProperties();
            //RaisePreviewProperties();
        }

        #endregion

        #region Validation

        public bool IsValid
        {
            get
            {
                return ValidateObjectName() == null
                    && ValidateSourceFile() == null
                    /*&& ValidateMaskLeft() == null
                    && ValidateMaskTop() == null
                    && ValidateMaskRight() == null
                    && ValidateMaskBottom() == null
                    && ValidateOriginX() == null
                    && ValidateOriginY() == null*/;
            }
        }

        private string ValidateObjectName ()
        {
            string txt = _textObjectName.Text.Trim();

            if (string.IsNullOrWhiteSpace(txt))
                return "Object Name must not be empty";
            if (_reservedNames.Contains(txt))
                return "Object Name conflicts with another Object";
            return null;
        }

        private string ValidateSourceFile ()
        {
            string txt = _textSource.Text.Trim();

            if (string.IsNullOrWhiteSpace(txt))
                return "Source File must not be empty";

            if (!_sourceFileValid)
                return "Invalid Source File slected";
            return null;
        }

        #endregion

        #region Object Preview Management

        private bool _sourceFileValid = false;
        private TextureResource _sourceImage;

        public TextureResource SourceImage
        {
            get { return _sourceImage; }
        }

        private void ClearObjectPreiew ()
        {
            _sourceImage = null;

            //RaisePreviewProperties();
        }

        private void LoadObjectPreview (String path)
        {
            try {
                _sourceImage = TextureResourceBitmapExt.CreateTextureResource(path);

                _originX = 0;
                _originY = 0;
                _maskLeft = 0;
                _maskTop = 0;
                _maskRight = _sourceImage.Width;
                _maskBottom = _sourceImage.Height;

                _sourceFileValid = true;
                UpdateMaskPropertyFields();
                //RaisePreviewProperties();
                //RaiseMaskProperties();
            }
            catch (Exception) {
                _sourceFileValid = false;
            }
        }

        private void ResetObjectPreview ()
        {
            if (!_validateController.ValidateForm())
                ClearObjectPreiew();

            if (_sourceFileValid)
                LoadObjectPreview(_sourceFile);
        }

        /*private void RaiseMaskProperties ()
        {
            RaisePropertyChanged("OriginX");
            RaisePropertyChanged("OriginY");
            RaisePropertyChanged("MaskLeft");
            RaisePropertyChanged("MaskTop");
            RaisePropertyChanged("MaskRight");
            RaisePropertyChanged("MaskBottom");
        }

        private void RaisePreviewProperties ()
        {
            RaisePropertyChanged("PreviewCanvasWidth");
            RaisePropertyChanged("PreviewCanvasHeight");
            RaisePropertyChanged("PreviewImageX");
            RaisePropertyChanged("PreviewImageY");
            RaisePropertyChanged("PreviewImageWidth");
            RaisePropertyChanged("PreviewImageHeight");
            RaisePropertyChanged("PreviewMaskX");
            RaisePropertyChanged("PreviewMaskY");
            RaisePropertyChanged("PreviewMaskWidth");
            RaisePropertyChanged("PreviewMaskHeight");
            RaisePropertyChanged("PreviewOriginX");
            RaisePropertyChanged("PreviewOriginY");
            RaisePropertyChanged("SourceImage");
        }*/

        private void UpdateMaskPropertyFields ()
        {
            if (_numOriginX.Value != (_originX ?? 0))
                _numOriginX.Value = _originX ?? 0;
            if (_numOriginY.Value != (_originY ?? 0))
                _numOriginY.Value = _originY ?? 0;
            if (_numMaskLeft.Value != (_maskLeft ?? 0))
                _numMaskLeft.Value = _maskLeft ?? 0;
            if (_numMaskRight.Value != (_maskRight ?? 0))
                _numMaskRight.Value = _maskRight ?? 0;
            if (_numMaskTop.Value != (_maskTop ?? 0))
                _numMaskTop.Value = _maskTop ?? 0;
            if (_numMaskBottom.Value != (_maskBottom ?? 0))
                _numMaskBottom.Value = _maskBottom ?? 0;
        }

        public int PreviewCanvasWidth
        {
            get { return 200; }
        }

        public int PreviewCanvasHeight
        {
            get { return 200; }
        }

        public int PreviewImageWidth
        {
            get { return _sourceImage != null ? _sourceImage.Width : 0; }
        }

        public int PreviewImageHeight
        {
            get { return _sourceImage != null ? _sourceImage.Height : 0; }
        }

        public int PreviewImageX
        {
            get { return PreviewCanvasWidth / 2 - PreviewImageWidth / 2; }
        }

        public int PreviewImageY
        {
            get { return PreviewCanvasHeight / 2 - PreviewImageHeight / 2; }
        }

        public int PreviewOriginX
        {
            get { return (PreviewImageX + (_originX ?? 0)) - 3; }
        }

        public int PreviewOriginY
        {
            get { return (PreviewImageY + (_originY ?? 0)) - 3; }
        }

        public int PreviewMaskX
        {
            get { return (PreviewImageX + (_originX ?? 0)) + (_maskLeft ?? 0); }
        }

        public int PreviewMaskY
        {
            get { return (PreviewImageY + (_originY ?? 0)) + (_maskTop ?? 0); }
        }

        public int PreviewMaskWidth
        {
            get { return (_maskRight ?? 0) - (_maskLeft ?? 0); }
        }

        public int PreviewMaskHeight
        {
            get { return (_maskBottom ?? 0) - (_maskTop ?? 0); }
        }

        #endregion

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            if (!_validateController.ValidateForm())
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void _buttonBrowse_Click (object sender, EventArgs e)
        {
            OpenSourceFile();
        }

        private void _textObjectName_TextChanged (object sender, EventArgs e)
        {
            ObjectName = _textObjectName.Text;
        }

        private void _numMaskLeft_ValueChanged (object sender, EventArgs e)
        {
            MaskLeft = (int)_numMaskLeft.Value;
        }

        private void _numMaskRight_ValueChanged (object sender, EventArgs e)
        {
            MaskRight = (int)_numMaskRight.Value;
        }

        private void _numMaskTop_ValueChanged (object sender, EventArgs e)
        {
            MaskTop = (int)_numMaskTop.Value;
        }

        private void _numMaskBottom_ValueChanged (object sender, EventArgs e)
        {
            MaskBottom = (int)_numMaskBottom.Value;
        }

        private void _numOriginX_ValueChanged (object sender, EventArgs e)
        {
            OriginX = (int)_numOriginX.Value;
        }

        private void _numOriginY_ValueChanged (object sender, EventArgs e)
        {
            OriginY = (int)_numOriginY.Value;
        }

        private string FindDefaultName (string baseName)
        {
            int i = 0;
            while (true) {
                string name = baseName + " " + ++i;
                if (_reservedNames.Contains(name)) {
                    continue;
                }
                return name;
            }
        }
    }
}
