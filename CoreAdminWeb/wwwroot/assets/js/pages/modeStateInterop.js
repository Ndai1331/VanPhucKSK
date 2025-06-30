@inject ModeStateService ModeStateService

@code {
    protected override async Task OnInitializedAsync()
    {
        ModeStateService.ModeChanged += HandleModeChanged;
    }

    private void HandleModeChanged()
    {
        SetBodyClass(ModeStateService.Mode);
    }

    private async Task SetBodyClass(string mode)
    {
        await JSRuntime.InvokeVoidAsync("modeStateInterop.setBodyClass", mode);
    }
}
