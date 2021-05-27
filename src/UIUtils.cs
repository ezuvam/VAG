using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace ezuvam.VAG
{

    public class VAMCustomUIElement
    {
        public readonly VAMCustomUIWnd OwnerWnd;
        public string Name { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visible { get { return GetVisible(); } set { SetVisible(value); } }
        public VAMCustomUIElement(VAMCustomUIWnd ownerWnd, string name, float width, float height)
        {
            OwnerWnd = ownerWnd;
            Name = name;
            Width = width;
            Height = height;
            OwnerWnd.RegisterUIElement(this);
        }

        protected virtual bool GetVisible()
        {
            return true;
        }
        protected virtual void SetVisible(bool value)
        {
            OwnerWnd.UIElementVisibleChanged(this);
        }
        public virtual void OnDestroy()
        {

        }

    }

    /*
    public class VAMCustomUIGameObjectElement : VAMCustomUIElement
    {
        protected readonly GameObject _gameObject;
        public VAMCustomUIGameObjectElement(VAMCustomUIWnd ownerWnd) : base(ownerWnd)
        {
            _gameObject = new GameObject();
            
            _gameObject.transform.parent = ownerWnd._wndGameObject.transform;
            _gameObject.transform.localScale = Vector3.one;
            _gameObject.transform.localPosition = Vector3.zero;
            _gameObject.transform.localRotation = Quaternion.identity;
        }
    }

    public class VAMUITextElement : VAMCustomUIGameObjectElement
    {
        private readonly Text _text;
        public VAMUITextElement(VAMCustomUIWnd ownerWnd) : base(ownerWnd)
        {
            _gameObject.name = "Text";

            _text = _gameObject.AddComponent<Text>();

            RectTransform rt = _gameObject.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);

            _text.alignment = TextAnchor.MiddleCenter;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;

            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            _text.font = ArialFont;
            _text.fontSize = 28;
            _text.text = "Test";
            _text.enabled = true;
            _text.color = Color.white;
        }
    }

    public class VAMUIButtonElement : VAMUITextElement
    {
        public VAMUIButtonElement(VAMCustomUIWnd ownerWnd) : base(ownerWnd)
        {

        }
    }
    */

    public class VAMCustomTransformUIElement : VAMCustomUIElement
    {
        public VAMCustomTransformUIElement(VAMCustomUIWnd ownerWnd, string name, float width, float height) : base(ownerWnd, name, width, height) { }

        private Transform _elementTransform;

        public Transform ElementTransform
        {
            get { return _elementTransform; }
            set
            {
                _elementTransform = value;

                UpdatePosition(_elementTransform);

                if (OwnerWnd != null)
                {
                    _elementTransform.SetParent(OwnerWnd.ContentContainer.transform, false);
                }
            }
        }
        protected void UpdatePosition(Transform t)
        {
            t.transform.position = Vector3.zero;
            RectTransform rt = t.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0); //new Vector2(Width / 2, Height / 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Height);
        }

        protected override bool GetVisible()
        {
            return (ElementTransform.gameObject != null) & ElementTransform.gameObject.activeSelf;
        }
        protected override void SetVisible(bool value)
        {
            if (ElementTransform.gameObject != null)
            {
                if (ElementTransform.gameObject.activeSelf != value)
                {
                    ElementTransform.gameObject.SetActive(value);
                    base.SetVisible(value);
                }
            }
        }
        public void SetParentTransform(Transform parent)
        {
            UpdatePosition(ElementTransform);
            ElementTransform.SetParent(parent, false);            
        }
        public void LookAtPlayer()
        {
            ElementTransform.LookAt(SuperController.singleton.navigationRig.transform);
        }
        public override void OnDestroy()
        {
            if (ElementTransform.gameObject != null) GameObject.Destroy(ElementTransform.gameObject);
            base.OnDestroy();
        }
    }

    public class VAMUIButton : VAMCustomTransformUIElement
    {
        public readonly UIDynamicButton button;
        public VAMUIButton(VAMCustomUIWnd ownerWnd, string name, string caption = "", float width = 100, float height = 35) : base(ownerWnd, name, width, height)
        {
            ElementTransform = GameObject.Instantiate<Transform>(ownerWnd.OwnerPlugin.manager.configurableButtonPrefab);

            button = ElementTransform.GetComponent<UIDynamicButton>();
            button.label = name;
            button.buttonText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            button.buttonText.text = caption;
            button.buttonText.fontSize = 16;

            //float xSpacing = 0.22f;
            //button.transform.Translate(xSpacing, 0.3f, 0, Space.Self);
        }

    }


    public class VAMUIWindowPopUpButton : VAMUIButton
    {
        public readonly VAMCustomUIWnd PopUpWnd;
        public VAMUIWindowPopUpButton(VAMCustomUIWnd ownerWnd, VAMCustomUIWnd popUpWnd, string name, string caption = "", float width = 100, float height = 35) : base(ownerWnd, name, caption, width, height)
        {
            PopUpWnd = popUpWnd;
            PopUpWnd.Visible = false;

            RectTransform PopUpRT = PopUpWnd.WndGameObject.GetComponent<RectTransform>();
            RectTransform OwnerWndRT = ownerWnd.WndGameObject.GetComponent<RectTransform>();
            PopUpRT.SetParent(OwnerWndRT, false);

            button.button.onClick.AddListener(() =>
            {
                if (!PopUpWnd.Visible)
                {
                    ShowPopUp();
                }
                else
                { PopUpWnd.Visible = false; }
            });
        }

        public void ShowPopUp()
        {
            OwnerWnd.PopUpButtonClicked(this);
            PlacePopUp();
            PopUpWnd.Visible = true;
        }

        public void PlacePopUp()
        {
            RectTransform refRt = OwnerWnd._imageContainer.GetComponent<RectTransform>();
            RectTransform popUpRt = PopUpWnd.WndGameObject.GetComponent<RectTransform>();

            popUpRt.localScale = refRt.localScale;

            // Workaround for strange case: Don't know why but we have 17.5 by each element if elements are UIButtons. I don't get it :(
            float newheight = (PopUpWnd.Height * popUpRt.localScale.y) - ((PopUpWnd._uiElements.Count - 1) * 17.5f);
            popUpRt.localPosition = new Vector3(refRt.localPosition.x, refRt.localPosition.y + newheight, refRt.localPosition.z);

            //OwnerWnd.OwnerPlugin.containingAtom.BroadcastMessage("DevToolsInspectUI", PopUpWnd.WndGameObject);            
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            PopUpWnd.OnDestroy();
        }
    }

    public class VAMCustomUIWnd
    {
        public readonly GameObject WndGameObject;
        private readonly GameObject _wndContainer;
        public readonly Canvas WndCanvas;
        public readonly GameObject _imageContainer;
        public readonly MVRScript OwnerPlugin;
        public List<VAMCustomUIElement> _uiElements;
        private bool _autoheight = false;
        public GameObject ContentContainer { get { return GetContentContainer(); } }
        public bool Visible { get { return WndGameObject.activeSelf; } set { SetVisible(value); } }
        public bool AutoHeight { get { return _autoheight; } set { _autoheight = value; if (value) { UpdateAutoHeight(); } } }
        private int _updateLockCount = 0;
        public bool IsUpdateLocked { get { return _updateLockCount != 0; } }
        public float Width
        {
            get
            {
                RectTransform imageRT = _imageContainer.GetComponent<RectTransform>();
                return imageRT.rect.width;
            }
            set
            {
                RectTransform imageRT = _imageContainer.GetComponent<RectTransform>();
                imageRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);

                RectTransform WndContainerRT = _wndContainer.GetComponent<RectTransform>();
                WndContainerRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            }
        }
        public float Height
        {
            get
            {
                RectTransform imageRT = _imageContainer.GetComponent<RectTransform>();
                return imageRT.rect.height;
            }
            set
            {
                RectTransform imageRT = _imageContainer.GetComponent<RectTransform>();
                imageRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);

                RectTransform WndContainerRT = _wndContainer.GetComponent<RectTransform>();
                WndContainerRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            }
        }
        public VAMCustomUIWnd(MVRScript ownerPlugin, float width = 500, float height = 500)
        {
            OwnerPlugin = ownerPlugin;
            _uiElements = new List<VAMCustomUIElement>();

            WndGameObject = new GameObject();
            WndCanvas = WndGameObject.AddComponent<Canvas>();
            WndCanvas.pixelPerfect = false;
            WndCanvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler cs = WndGameObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 100.0f;
            cs.dynamicPixelsPerUnit = 1f;

            GraphicRaycaster gr = WndGameObject.AddComponent<GraphicRaycaster>();

            //float scale = 0.002f;
            //WndCanvas.transform.localScale = new Vector3(scale, scale, scale);
            //WndCanvas.transform.Translate(0, 0.2f, 0);

            RectTransform WndGameObjectRT = WndGameObject.GetComponent<RectTransform>();
            WndGameObjectRT.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);

            WndGameObjectRT.pivot = new Vector2(0.5f, 0.5f);
            WndGameObjectRT.anchorMin = new Vector2(0, 1);
            WndGameObjectRT.anchorMax = new Vector2(1, 0);

            // this is the window background image
            _imageContainer = new GameObject();
            _imageContainer.transform.SetParent(WndGameObjectRT, false);

            Image image = _imageContainer.AddComponent<Image>();
            RectTransform imageRT = image.GetComponent<RectTransform>();
            imageRT.anchoredPosition = new Vector2(0, 0);
            imageRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            imageRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            image.color = new Color(0.2f, 0.2f, 0.8f);

            // continer for elements
            _wndContainer = new GameObject();
            RectTransform WndContainerRT = _wndContainer.AddComponent<RectTransform>();
            WndContainerRT.anchoredPosition = new Vector2(0, 0);
            WndContainerRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            WndContainerRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            WndContainerRT.SetParent(imageRT, false);


            //AspectRatioFitter asf = _wndContainer.AddComponent<AspectRatioFitter>();           
            //asf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;        

            /*
            LayoutElement le = _wndContainer.AddComponent<LayoutElement>();
            le.preferredWidth = 10;
            le.preferredHeight = 10;
            le.flexibleHeight = 1;
            le.flexibleWidth = 1;
            */

            //ContentSizeFitter csf = _wndContainer.AddComponent<ContentSizeFitter>();
            //csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            //csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        }

        public void BeginUpdate()
        {
            _updateLockCount += 1;
        }
        public void EndUpdate()
        {
            _updateLockCount -= 1;

            if (_updateLockCount == 0)
            {
                if (AutoHeight)
                {
                    UpdateAutoHeight();
                }
            }
        }
        protected virtual void SetVisible(bool value)
        {
            WndGameObject.SetActive(value);
        }

        public virtual void PopUpButtonClicked(VAMUIWindowPopUpButton popUpButton)
        {
            for (int i = 0; i < _uiElements.Count; i++)
            {
                if (!System.Object.Equals(_uiElements[i], popUpButton) & (_uiElements[i] is VAMUIWindowPopUpButton))
                {
                    (_uiElements[i] as VAMUIWindowPopUpButton).PopUpWnd.Visible = false;
                }
            }
        }

        protected GameObject GetContentContainer()
        {
            return _wndContainer;
        }

        public virtual void UIElementVisibleChanged(VAMCustomUIElement element)
        {
            if (AutoHeight)
            {
                UpdateAutoHeight();
            }
        }
        public void UpdateAutoHeight()
        {
            if (!IsUpdateLocked)
            {
                float prefheight = 0;
                for (int i = 0; i < _uiElements.Count; i++)
                {
                    if (_uiElements[i].Visible)
                    {
                        prefheight += _uiElements[i].Height;
                    }
                }
                Height = prefheight;
            }
        }
        public void RegisterUIElement(VAMCustomUIElement element)
        {
            _uiElements.Add(element);
        }

        public void AutoPlace()
        {
            {
                if (XRSettings.enabled == false)
                {
                    AttachToHUD();
                }
                else
                {
                    AttachToLeftHand();
                }
            }
        }

        public void Update()
        {
            if (XRSettings.enabled == false)
            {
                float horizontalHUDOffset = 0.85f; // 0.5 = middle of screen
                float verticalHUDOffset = 0.25f;  // 0.5 = middle of screen
                float depthHUDOffset = 1;

                Transform cameraT = SuperController.singleton.lookCamera.transform;
                Vector3 facingPos = cameraT.position + cameraT.forward * 10000000.0f;
                Vector3 endPos = Camera.main.ViewportToWorldPoint(new Vector3(horizontalHUDOffset, verticalHUDOffset, depthHUDOffset));
                WndCanvas.transform.LookAt(facingPos, Vector3.up);
                WndCanvas.transform.position = endPos;
            }
            else
            {
                // 
            }
        }
        public void AttachToHUD()
        {

        }
        public void AttachToLeftHand()
        {
            RectTransform rt = WndGameObject.GetComponent<RectTransform>();
            rt.Rotate(90, 20, 90);

            Transform ControllerTr = SuperController.singleton.leftControllerCamera.transform;

            rt.SetParent(ControllerTr, false);

            WndGameObject.transform.localPosition = new Vector3(-0.1f, -0.1f, -0.1f);
        }

        public void SetUIInteractive()
        {
            // only use AddCanvas if you want to interact with the UI - no needed if display only
            SuperController.singleton.AddCanvas(WndCanvas);
        }

        public void OnDestroy()
        {
            if (SuperController.singleton != null)
            {
                SuperController.singleton.RemoveCanvas(WndCanvas);
            }

            for (int i = 0; i < _uiElements.Count; i++)
            {
                _uiElements[i].OnDestroy();
            }

            WndCanvas.transform.SetParent(null, false);
            if (WndGameObject != null) GameObject.Destroy(WndGameObject);
            if (_wndContainer != null) GameObject.Destroy(_wndContainer);
        }
        public void Test()
        {

        }

    }

    public class VAMCustomVerticalLayoutUIWnd : VAMCustomUIWnd
    {
        public VAMCustomVerticalLayoutUIWnd(MVRScript plugin, float width = 500, float height = 500) : base(plugin, width, height)
        {

            //ContentSizeFitter csf = ContentContainer.AddComponent<ContentSizeFitter>();
            //csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            //csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = ContentContainer.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
        }
    }

    public class VAMCustomHorizontalLayoutUIWnd : VAMCustomUIWnd
    {
        public VAMCustomHorizontalLayoutUIWnd(MVRScript plugin, float width = 500, float height = 500) : base(plugin, width, height)
        {

            //ContentSizeFitter csf = ContentContainer.AddComponent<ContentSizeFitter>();
            //csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            //csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            HorizontalLayoutGroup vlg = ContentContainer.AddComponent<HorizontalLayoutGroup>();
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = true;
            vlg.childForceExpandWidth = false;
        }
    }

}