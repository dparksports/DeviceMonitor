Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase

# XAML for the UI
$xaml = @"
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Pomodoro" Height="420" Width="320"
        WindowStartupLocation="CenterScreen"
        Background="#FF202020"
        ResizeMode="NoResize">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="40"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Title -->
        <TextBlock Grid.Row="0"
                   Text="Pomodoro"
                   HorizontalAlignment="Center"
                   Margin="0,5,0,10"
                   FontSize="32"
                   FontWeight="Bold"
                   Foreground="#FF4AA3FF"/>

        <!-- Top buttons -->
        <StackPanel Grid.Row="1"
                    Orientation="Horizontal"
                    HorizontalAlignment="Center"
                    Margin="0,0,0,10">
            <Button x:Name="btnDone"
                    Content="Done"
                    Margin="5,0"
                    Padding="12,4"
                    Background="#FF333333"
                    Foreground="White"
                    BorderBrush="#FF555555"
                    FontWeight="SemiBold"/>
            <Button x:Name="btnNewTask"
                    Content="New Task"
                    Margin="5,0"
                    Padding="12,4"
                    Background="#FF333333"
                    Foreground="White"
                    BorderBrush="#FF555555"
                    FontWeight="SemiBold"/>
            <Button x:Name="btnTimer"
                    Content="Timer"
                    Margin="5,0"
                    Padding="12,4"
                    Background="#FFFFA500"
                    Foreground="Black"
                    BorderBrush="#FFFFC040"
                    FontWeight="SemiBold"/>
        </StackPanel>

        <!-- Timer display -->
        <Border Grid.Row="2"
                Margin="0,10,0,5"
                Padding="10"
                Background="#FF101010"
                BorderBrush="#FFFFA500"
                BorderThickness="2"
                CornerRadius="6">
            <StackPanel>
                <TextBlock x:Name="txtTimer"
                           Text="00000"
                           HorizontalAlignment="Center"
                           FontSize="48"
                           FontWeight="Bold"
                           Foreground="#FFFFD000"
                           FontFamily="Consolas"/>
                <StackPanel Orientation="Horizontal"
                            HorizontalAlignment="Center"
                            Margin="0,5,0,0">
                    <TextBlock Text="mins"
                               Margin="0,0,10,0"
                               Foreground="#FFFFA500"
                               FontSize="14"/>
                    <TextBlock Text="secs"
                               Foreground="#FFFFA500"
                               FontSize="14"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- Total Used -->
        <Border Grid.Row="3"
                Margin="0,10,0,0"
                Padding="10"
                Background="#FF101010"
                BorderBrush="#FFFFA500"
                BorderThickness="2"
                CornerRadius="6">
            <StackPanel>
                <TextBlock Text="Total Used"
                           HorizontalAlignment="Center"
                           FontSize="16"
                           FontWeight="SemiBold"
                           Foreground="#FFFFA500"
                           Margin="0,0,0,5"/>
                <TextBlock x:Name="txtTotalUsed"
                           Text="000000"
                           HorizontalAlignment="Center"
                           FontSize="28"
                           FontWeight="Bold"
                           Foreground="#FFFFD000"
                           FontFamily="Consolas"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
"@

# Load XAML
$reader = New-Object System.Xml.XmlNodeReader ([xml]$xaml)
$window = [Windows.Markup.XamlReader]::Load($reader)

# Get controls
$btnDone = $window.FindName('btnDone')
$btnNewTask = $window.FindName('btnNewTask')
$btnTimer = $window.FindName('btnTimer')
$txtTimer = $window.FindName('txtTimer')
$txtTotalUsed = $window.FindName('txtTotalUsed')

# Simple state
$secondsRemaining = 0
$totalUsed = 0

# Timer object
$dispatcherTimer = New-Object System.Windows.Threading.DispatcherTimer
$dispatcherTimer.Interval = [TimeSpan]::FromSeconds(1)

$dispatcherTimer.Add_Tick({
        if ($secondsRemaining -le 0) {
            $dispatcherTimer.Stop()
            return
        }
        $secondsRemaining--
        $txtTimer.Text = "{0:00000}" -f $secondsRemaining
    })

# Button events
$btnTimer.Add_Click({
        # Example: start a 25-minute Pomodoro
        $secondsRemaining = 25 * 60
        $txtTimer.Text = "{0:00000}" -f $secondsRemaining
        $dispatcherTimer.Start()
    })

$btnDone.Add_Click({
        if ($secondsRemaining -gt 0) {
            $dispatcherTimer.Stop()
            $secondsRemaining = 0
            $txtTimer.Text = "00000"
        }
        $totalUsed++
        $txtTotalUsed.Text = "{0:000000}" -f $totalUsed
    })

$btnNewTask.Add_Click({
        # Visual reset; you can hook this to your own logic
        $dispatcherTimer.Stop()
        $secondsRemaining = 0
        $txtTimer.Text = "00000"
    })

# Show window
$window.ShowDialog() | Out-Null
