﻿''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
NotInheritable Class App
    Inherits Application

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Async Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)

        If e.TileId.IndexOf("nullTile") = 0 Then
            ' czyli to moja tile
            If e.Arguments.Length > 2 Then
                If e.Arguments.Substring(0, 4).ToLower = "http" Then
                    OpenBrowser(e.Arguments, False)
                Else
                    Dim oFold As Windows.Storage.StorageFolder = Windows.Storage.KnownFolders.PicturesLibrary
                    Dim sFile As String = e.Arguments

                    ' najpierw przejdziemy przez podkatalogi
                    Dim iInd As Integer
                    iInd = sFile.IndexOf("\")
                    While iInd > 0
                        Dim sFold As String = sFile.Substring(0, iInd)
                        oFold = Await oFold.GetFolderAsync(sFold)
                        sFile = sFile.Substring(iInd + 1)
                        iInd = sFile.IndexOf("\")
                    End While

                    'a teraz juz koncowy plik
                    Dim oFile As Windows.Storage.StorageFile
                    oFile = Await oFold.TryGetItemAsync(sFile)

                    If oFile Is Nothing Then Exit Sub
                    Windows.System.Launcher.LaunchFileAsync(oFile)
                End If
            End If
            Exit Sub
        End If

        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub
#Region "Get/Set settings"

#Region "String"

    Public Shared Function GetSettingsString(sName As String, Optional sDefault As String = "") As String
        Dim sTmp As String

        sTmp = sDefault

        With Windows.Storage.ApplicationData.Current
            If .RoamingSettings.Values.ContainsKey(sName) Then
                sTmp = .RoamingSettings.Values(sName).ToString
            End If
            If .LocalSettings.Values.ContainsKey(sName) Then
                sTmp = .LocalSettings.Values(sName).ToString
            End If
        End With

        Return sTmp

    End Function

    Public Shared Sub SetSettingsString(sName As String, sValue As String)
        SetSettingsString(sName, sValue, False)
    End Sub

    Public Shared Sub SetSettingsString(sName As String, sValue As String, bRoam As Boolean)
        With Windows.Storage.ApplicationData.Current
            If bRoam Then .RoamingSettings.Values(sName) = sValue
            .LocalSettings.Values(sName) = sValue
        End With
    End Sub
#End Region
#Region "Int"
    Public Shared Function GetSettingsInt(sName As String, Optional iDefault As Integer = 0) As Integer
        Dim sTmp As Integer

        sTmp = iDefault

        With Windows.Storage.ApplicationData.Current
            If .RoamingSettings.Values.ContainsKey(sName) Then
                sTmp = CInt(.RoamingSettings.Values(sName).ToString)
            End If
            If .LocalSettings.Values.ContainsKey(sName) Then
                sTmp = CInt(.LocalSettings.Values(sName).ToString)
            End If
        End With

        Return sTmp

    End Function

    Public Shared Sub SetSettingsInt(sName As String, sValue As Integer)
        SetSettingsInt(sName, sValue, False)
    End Sub

    Public Shared Sub SetSettingsInt(sName As String, sValue As Integer, bRoam As Boolean)
        With Windows.Storage.ApplicationData.Current
            If bRoam Then .RoamingSettings.Values(sName) = sValue.ToString
            .LocalSettings.Values(sName) = sValue.ToString
        End With
    End Sub
#End Region
#Region "Bool"
    Public Shared Function GetSettingsBool(sName As String, Optional iDefault As Boolean = False) As Boolean
        Dim sTmp As Boolean

        sTmp = iDefault
        With Windows.Storage.ApplicationData.Current
            If .RoamingSettings.Values.ContainsKey(sName) Then
                sTmp = CBool(.RoamingSettings.Values(sName).ToString)
            End If
            If .LocalSettings.Values.ContainsKey(sName) Then
                sTmp = CBool(.LocalSettings.Values(sName).ToString)
            End If
        End With

        Return sTmp

    End Function
    Public Shared Sub SetSettingsBool(sName As String, sValue As Boolean)
        SetSettingsBool(sName, sValue, False)
    End Sub

    Public Shared Sub SetSettingsBool(sName As String, sValue As Boolean, bRoam As Boolean)
        With Windows.Storage.ApplicationData.Current
            If bRoam Then .RoamingSettings.Values(sName) = sValue.ToString
            .LocalSettings.Values(sName) = sValue.ToString
        End With
    End Sub
#End Region

#End Region

    Public Shared Sub OpenBrowser(oUri As Uri, bForceEdge As Boolean)
        If bForceEdge Then
            Dim options As Windows.System.LauncherOptions = New Windows.System.LauncherOptions()
            options.TargetApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe"
            Windows.System.Launcher.LaunchUriAsync(oUri, options)
        Else
            Windows.System.Launcher.LaunchUriAsync(oUri)
        End If

    End Sub
    Public Shared Sub OpenBrowser(sUri As String, bForceEdge As Boolean)
        Try
            Dim oUri As Uri = New Uri(sUri)
            OpenBrowser(oUri, bForceEdge)
        Catch ex As Exception
            ' jakby sUri nie był jednak poprawnym Uri... to nie wylatuj z errorem, tylko zignoruj
        End Try
    End Sub

    Public Shared Async Function DialogBox(sMsg As String) As Task
        Dim oMsg As Windows.UI.Popups.MessageDialog = New Windows.UI.Popups.MessageDialog(sMsg)
        Await oMsg.ShowAsync
    End Function

    Public Shared Function GetLangString(sMsg As String) As String
        If sMsg = "" Then Return ""

        Dim sRet As String = sMsg
        Try
            sRet = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(sMsg)
        Catch
        End Try
        Return sRet
    End Function

    Public Shared Async Sub DialogBoxRes(sMsg As String)
        sMsg = GetLangString(sMsg)
        Await DialogBox(sMsg)
    End Sub


End Class
