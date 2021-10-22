
' STORE 2.1909.1 - 2019.09

' 2019.08.29
' * podawanie numeru wersji
' * możliwość otwarcia obrazka (a nie strony WWW)
' * możliwość wpisania numerka (po DoubleClick na informacji o numerze)

' STORE 1.2.8 - 2019.02


Public NotInheritable Class MainPage
    Inherits Page

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiCreatedNum.Text = App.GetLangString("resCreatedNum") & ": " & App.GetSettingsInt("currentTile")
        uiVers.Text = Windows.ApplicationModel.Package.Current.Id.Version.Major & "." &
                Windows.ApplicationModel.Package.Current.Id.Version.Minor & "." &
                Windows.ApplicationModel.Package.Current.Id.Version.Build
    End Sub

    Private Async Sub uiCreate_Click(sender As Object, e As RoutedEventArgs)

        Dim sUrl As String = uiTileUrl.Text
        If sUrl.Trim.Length < 5 Then
            sUrl = ""
            uiTileUrl.Text = sUrl
        End If

        Dim oUri As Uri = Nothing
        If sUrl <> "" Then
            Try
                oUri = New Uri(sUrl)
            Catch ex As Exception
                oUri = Nothing
            End Try

            If oUri Is Nothing Then
                App.DialogBoxRes("errBadUri")
                Exit Sub
            End If
        End If

        Dim sFile As String = uiPictPath.Text
        If sFile <> "" AndAlso sUrl <> "" Then
            App.DialogBoxRes("errPicOrUri")
            Exit Sub
        End If

        If sFile <> "" Then
            sUrl = sFile
            uiPictPath.Text = ""    ' żeby nie było powtórki?
        End If

        Dim iNum As Integer = App.GetSettingsInt("currentTile")
        App.SetSettingsInt("currentTile", iNum + 1)
        uiCreatedNum.Text = App.GetLangString("resCreatedNum") & ": " & iNum + 1

        Dim sName As String = "nullTile" & iNum
        Dim sTxt As String = uiTileText.Text
        If sUrl = "" Then sUrl = " "
        Dim sSize As String = TryCast(uiTileSize.SelectedValue, ComboBoxItem).Content
        Dim sArgs As String = sUrl ' uiTileUrl.Text
        'If sArgs = "" Then sArgs = " "

        Dim oSTile As Windows.UI.StartScreen.SecondaryTile
        ' IDtile: 64 znaki
        ' arguments: 2048
        oSTile = New Windows.UI.StartScreen.SecondaryTile(sName, sName, sArgs, New Uri("ms-appx:///Assets/empty.png"), Windows.UI.StartScreen.TileSize.Square150x150)
        Dim isPinned As Boolean = Await oSTile.RequestCreateAsync()

        If Not isPinned Then Exit Sub

        Dim sLine As String
        sLine = "<text hint-style='" & sSize & "' hint-align='center'>" & sTxt & "</text>"

        Dim sTmp As String
        sTmp = "<tile><visual>"

        sTmp = sTmp & "<binding template ='TileSmall' branding='none' hint-textStacking='center'>"
        sTmp = sTmp & sLine
        sTmp = sTmp & "</binding>"

        sTmp = sTmp & "<binding template ='TileMedium' branding='none' hint-textStacking='center'>"
        sTmp = sTmp & sLine
        sTmp = sTmp & "</binding>"

        sTmp = sTmp & "<binding template ='TileWide' branding='none' hint-textStacking='center'>"
        sTmp = sTmp & sLine
        sTmp = sTmp & "</binding>"

        sTmp = sTmp & "<binding template ='TileLarge' branding='none' hint-textStacking='center'>"
        sTmp = sTmp & sLine
        sTmp = sTmp & "</binding>"

        sTmp = sTmp & "</visual></tile>"

        Dim oTile As Windows.UI.Notifications.TileNotification

        Dim oXml As Windows.Data.Xml.Dom.XmlDocument = New Windows.Data.Xml.Dom.XmlDocument
        oXml.LoadXml(sTmp)
        oTile = New Windows.UI.Notifications.TileNotification(oXml)

        Dim oTUPS As Windows.UI.Notifications.TileUpdater
        oTUPS = Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForSecondaryTile(sName)
        oTUPS.Clear()

        If sTmp IsNot Nothing Then
            oTUPS.Update(oTile)
        End If

    End Sub

    Private Async Sub uiPictBrowse_Click(sender As Object, e As RoutedEventArgs) Handles uiPictBrowse.Click

        Dim picker = New Windows.Storage.Pickers.FileOpenPicker()
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
        picker.FileTypeFilter.Add(".jpg")
        picker.FileTypeFilter.Add(".jpeg")
        picker.FileTypeFilter.Add(".png")

        Dim oFile As Windows.Storage.StorageFile
        oFile = Await picker.PickSingleFileAsync
        If oFile Is Nothing Then Exit Sub

        ' HARD założenie: obcinamy do drugiego\ włącznie
        Dim sPath As String = oFile.Path
        Dim iInd As Integer
        iInd = sPath.IndexOf("\")
        If iInd <> 2 Then Exit Sub    ' że nie X:\ 
        sPath = sPath.Substring(iInd + 1)
        iInd = sPath.IndexOf("\")
        If iInd < 1 Then Exit Sub    ' że nie ma kolejnego katalogu? d:\pictures
        sPath = sPath.Substring(iInd + 1)


        uiPictPath.Text = sPath
        ' kopiuj plik do siebie, w takie miejsce zeby potem Image potrafilo wziac :)

    End Sub



    Private Sub uiCreatedNum_DoubleTapped(sender As Object, e As DoubleTappedRoutedEventArgs) Handles uiCreatedNum.DoubleTapped
        ' przełączenie pola
        uiSetNum.Visibility = Visibility.Visible
    End Sub

    Private Sub uiSetNum_Click(sender As Object, e As RoutedEventArgs)
        uiSetNum.Visibility = Visibility.Collapsed
        Dim iNum As Integer
        If Not Integer.TryParse(uiNewNum.Text, iNum) Then Exit Sub

        App.SetSettingsInt("currentTile", iNum)
        uiCreatedNum.Text = App.GetLangString("resCreatedNum") & ": " & iNum
    End Sub
End Class
