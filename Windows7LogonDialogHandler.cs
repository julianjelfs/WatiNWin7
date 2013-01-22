using System.Linq;
using System.Windows.Automation;
using WatiN.Core.DialogHandlers;
using WatiN.Core.Native.Windows;

namespace SystemTests
{
	//class for handling windows 7 logon in Watin
    public class Windows7LogonDialogHandler : BaseDialogHandler
    {
        private readonly string _username;
        private readonly string _password;
        
        readonly AndCondition _listCondition = new AndCondition(new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                                                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));

        readonly AndCondition _editCondition = new AndCondition(new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                                                                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

        readonly AndCondition _buttonConditions = new AndCondition(new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                                                                   new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

        public Windows7LogonDialogHandler(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public override bool HandleDialog(Window window)
        {
            if (CanHandleDialog(window))
            {
                var win = AutomationElement.FromHandle(window.Hwnd);
                var lists = win.FindAll(TreeScope.Children, _listCondition);
                var buttons = win.FindAll(TreeScope.Children, _buttonConditions);
                var another = (from AutomationElement list in lists
                               where list.Current.ClassName == "UserTile"
                               where list.Current.Name == "Use another account"
                               select list).First();
                another.SetFocus();

                foreach (var edit in from AutomationElement list in lists
                                     where list.Current.ClassName == "UserTile"
                                     select list.FindAll(TreeScope.Children, _editCondition)
                                         into edits
                                         from AutomationElement edit in edits
                                         select edit)
                {
                    if (edit.Current.Name.Contains("User name"))
                    {
                        edit.SetFocus();
                        var usernamePattern = edit.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                        if (usernamePattern != null) usernamePattern.SetValue(_username);
                    }
                    if (edit.Current.Name.Contains("Password"))
                    {
                        edit.SetFocus();
                        var passwordPattern = edit.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                        if (passwordPattern != null) passwordPattern.SetValue(_password);
                    }
                }
                foreach (var submitPattern in from AutomationElement button in buttons
                                              where button.Current.AutomationId == "SubmitButton"
                                              select button.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern)
                {
                    submitPattern.Invoke();
                    break;
                }
                return true;
            }
            return false;
        }

        public override bool CanHandleDialog(Window window)
        {
            return window.ClassName == "#32770";
        }
    }
}