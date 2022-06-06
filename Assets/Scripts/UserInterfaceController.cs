using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceController : MonoBehaviour
{
    private VisualElement _menu;
    private VisualElement _mainNav;
    private VisualElement[] _mainMenuOptions;
    private List<VisualElement> _widgets;
    private Button _addButton;
    private Button _removeButton;
    private TextField _textInput;
    private VisualElement _inputContainer;
    private VisualElement _widget1;
    private VisualElement _widget2;
    private RadioButtonGroup _radioButtonGroup1;
    private RadioButtonGroup _radioButtonGroup2;
    private const string POPUP_ANIMATION = "pop-animation-hide";
    private int _mainPopupIndex = -1;
    private StyleTranslate _zeroPositionTranslate = new(new Translate(0, 0, 0));
    private int _radioOptions;
    private void Awake()
    {
        GetElementReferences();
        AddListeners();
        SetInitialValues();
    }

    private void GetElementReferences()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _menu = root.Q<VisualElement>("Menu");
        _mainNav = root.Q<VisualElement>("MainNav");
        _inputContainer = root.Q<VisualElement>("InputContainer");
        _widgets = root.Q<VisualElement>("WidgetsContainer").Children().ToList();
        _widget1 = root.Q<VisualElement>("WidgetContainer", "Widget1");
        _widget2 = root.Q<VisualElement>("WidgetContainer", "Widget2");
        _radioButtonGroup1 = _widget1.Q<RadioButtonGroup>();
        _radioButtonGroup2 = _widget2.Q<RadioButtonGroup>();
        _textInput = root.Q<TextField>("TextInput");
        _addButton = root.Q<Button>("AddButton");
        _removeButton = root.Q<Button>("RemoveButton");
        RefreshMenuOptions();
    }

    private void AddListeners()
    {
        _addButton.clicked += OnAddButtonClicked;
        _removeButton.clicked += OnRemoveButtonClicked;
        _radioButtonGroup1.RegisterValueChangedCallback(OnWidgetValueChange);
        _radioButtonGroup2.RegisterValueChangedCallback(OnWidgetValueChange);
        _menu.RegisterCallback<TransitionEndEvent>(OnMenuTransitionEnd);
        _inputContainer.RegisterCallback<TransitionEndEvent>(OnInputTransitionEnd);
    }

    private void SetInitialValues()
    {
        _radioOptions = _radioButtonGroup1.choices.Count();
        SetWidgetImage(_radioButtonGroup1.value, null, _widget1);
        SetWidgetImage(_radioButtonGroup2.value + _radioOptions, null, _widget2);
    }

    private void OnWidgetValueChange(ChangeEvent<int> evt)
    {
        var evtCurrentTarget = (VisualElement)evt.currentTarget;
        var index = evtCurrentTarget == _radioButtonGroup2 ? _radioOptions : 0;
        var changeTarget = evtCurrentTarget.parent.parent;
        SetWidgetImage(evt.newValue + index, evt.previousValue + index, changeTarget);
    }

    private void SetWidgetImage(int value, int? previousValue, VisualElement widget)
    {
        var imageElement = widget.Q<VisualElement>("Image");
        
        if (previousValue != null)
        {
            imageElement.RemoveFromClassList("image"+(previousValue+1));
        }
        
        imageElement.AddToClassList("image"+(value+1));
    }
    
    private IEnumerator Start() 
    {
        yield return new WaitForSeconds(1f);
        _menu.ToggleInClassList(POPUP_ANIMATION);
    }

    private void OnAddButtonClicked()
    {
        var newMenuOption = new Label
        {
            text = _textInput.value,
            name = "Option"
        };
        
        _mainNav.Add(newMenuOption);
        RefreshMenuOptions();
    }

    private void OnRemoveButtonClicked()
    {
        foreach (var visualElement in _mainMenuOptions)
        {
            var option = (Label) visualElement;
            
            if (option.text == _textInput.value)
            {
                _mainNav.Remove(option);
                RefreshMenuOptions();
                break;
            }
        }
    }
    
    private void OnMenuTransitionEnd(TransitionEndEvent evt) 
    {
        if (!evt.stylePropertyNames.Contains("opacity")) { return; }
        if (_mainPopupIndex < _mainMenuOptions.Length -1)
        {
            _mainPopupIndex++;
            _mainMenuOptions[_mainPopupIndex].ToggleInClassList(POPUP_ANIMATION);
        }
        else
        {
            _inputContainer.style.translate = _zeroPositionTranslate;
        }
    }

    private void OnInputTransitionEnd(TransitionEndEvent evt)
    {
        _widgets.ForEach(x => x.style.translate = _zeroPositionTranslate);
    }
    
    private void RefreshMenuOptions()
    {
        _mainMenuOptions = _mainNav.Children().ToArray();
    }
}
