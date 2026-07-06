using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO.Packaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Task = System.Threading.Tasks.Task;

namespace Mita.GitHubCopilotRtl
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MakeGitHubCopilotRtl
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int EnableCommandId = 0x0100;
        public const int DisableCommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("159657b2-32b7-453a-aabf-0d2b8f82e925");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MakeGitHubCopilotRtl"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MakeGitHubCopilotRtl(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var enableMenuCommandID = new CommandID(CommandSet, EnableCommandId);
            var enableMenuItem = new MenuCommand(this.ExecuteEnable, enableMenuCommandID);
            commandService.AddCommand(enableMenuItem);

            var disableMenuCommandID = new CommandID(CommandSet, DisableCommandId);
            var disableMenuItem = new MenuCommand(this.ExecuteDisable, disableMenuCommandID);
            commandService.AddCommand(disableMenuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MakeGitHubCopilotRtl Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MakeGitHubCopilotRtl's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new MakeGitHubCopilotRtl(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ExecuteEnable(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RTLHelper.TryDetectCopilotChatAndApplyDirection(true);
        }
        private void ExecuteDisable(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            RTLHelper.TryDetectCopilotChatAndApplyDirection(false);
        }
    }
}
