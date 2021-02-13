using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace ModTools.UI
{
    public class UIKeymappingsPanel : UICustomControl
    {
        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate";
        private SavedInputKey editingBinding;
        private int count;

        internal UIComponent AddKeymapping(string label, SavedInputKey savedInputKey)
        {
            UIPanel uipanel = base.component.AttachUIComponent(UITemplateManager.GetAsGameObject(UIKeymappingsPanel.kKeyBindingTemplate)) as UIPanel;
            int num = this.count;
            this.count = num + 1;
            if (num % 2 == 1)
            {
                uipanel.backgroundSprite = null;
            }
            UILabel uilabel = uipanel.Find<UILabel>("Name");
            UIButton uibutton = uipanel.Find<UIButton>("Binding");
            uibutton.eventKeyDown += this.OnBindingKeyDown;
            uibutton.eventMouseDown += this.OnBindingMouseDown;
            uilabel.text = label;
            uibutton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uibutton.objectUserData = savedInputKey;
            return uipanel;
        }

        private bool IsModifierKey(KeyCode code)
        {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        private bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private bool IsAltDown()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private bool IsUnbindableMouseButton(UIMouseButton code)
        {
            return code == UIMouseButton.Left || code == UIMouseButton.Right;
        }

        private KeyCode ButtonToKeycode(UIMouseButton button)
        {
            if (button == UIMouseButton.Left)
            {
                return KeyCode.Mouse0;
            }
            if (button == UIMouseButton.Right)
            {
                return KeyCode.Mouse1;
            }
            if (button == UIMouseButton.Middle)
            {
                return KeyCode.Mouse2;
            }
            if (button == UIMouseButton.Special0)
            {
                return KeyCode.Mouse3;
            }
            if (button == UIMouseButton.Special1)
            {
                return KeyCode.Mouse4;
            }
            if (button == UIMouseButton.Special2)
            {
                return KeyCode.Mouse5;
            }
            if (button == UIMouseButton.Special3)
            {
                return KeyCode.Mouse6;
            }
            return KeyCode.None;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (this.editingBinding != null && !this.IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey value = (p.keycode == KeyCode.Escape) ? this.editingBinding.value : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace)
                {
                    value = SavedInputKey.Empty;
                }
                this.editingBinding.value = value;
                (p.source as UITextComponent).text = this.editingBinding.ToLocalizedString("KEYNAME");
                this.editingBinding = null;
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (this.editingBinding == null)
            {
                p.Use();
                this.editingBinding = (SavedInputKey)p.source.objectUserData;
                UIButton uibutton = p.source as UIButton;
                uibutton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uibutton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
                return;
            }
            if (!this.IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                InputKey value = SavedInputKey.Encode(this.ButtonToKeycode(p.buttons), this.IsControlDown(), this.IsShiftDown(), this.IsAltDown());
                this.editingBinding.value = value;
                UIButton uibutton2 = p.source as UIButton;
                uibutton2.text = this.editingBinding.ToLocalizedString("KEYNAME");
                uibutton2.buttonsMask = UIMouseButton.Left;
                this.editingBinding = null;
            }
        }

        internal InputKey GetDefaultEntry(string entryName)
        {
            FieldInfo field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                return 0;
            }
            object value = field.GetValue(null);
            if (value is InputKey)
            {
                return (InputKey)value;
            }
            return 0;
        }
    }
}
