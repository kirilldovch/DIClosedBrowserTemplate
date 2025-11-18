using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DIClosedBrowserTemplate.Models.Messages;

public class UpdateBrowserPageMessage(string? url = null) : ValueChangedMessage<string?>(url);