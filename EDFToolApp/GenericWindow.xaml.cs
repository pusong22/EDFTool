using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows;

namespace EDFToolApp
{
    /// <summary>
    /// Interaction logic for GenericWindow.xaml
    /// </summary>
    public partial class GenericWindow : Window, IRecipient<ValueChangedMessage<bool>>
    {
        public GenericWindow()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(ValueChangedMessage<bool> message)
        {
            DialogResult = message.Value;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            WeakReferenceMessenger.Default.Unregister<ValueChangedMessage<bool>>(this);
        }
    }
}
