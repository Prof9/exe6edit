Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Globalization
Imports System.Reflection
Imports System.Threading
Imports EXE6DataList

Public Class MainForm
    Private fname As String
    Private be As EXE6SaveDataEditor

    Private loadFlag As Integer = lFlag.NONE

    Private enabled As Boolean

    'ロードフラグ列挙体
    Enum lFlag
        NONE = 0    '未ロード
        VBA = 1     'VisualBoyAdvance Battery File
        PAR = 2     'ProActionReplay Save Data File
        GS = 3      'GameShark SnapShot File
    End Enum

    Public Sub New()
        MyBase.New()

        'Set previously chosen language.
        Dim lang As String = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
        If My.Settings.Language IsNot Nothing And My.Settings.Language.Length > 0 Then
            lang = My.Settings.Language
            Thread.CurrentThread.CurrentUICulture = New CultureInfo(My.Settings.Language)
        End If

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Check menu item corresponding to currently chosen language.
        Dim found As Boolean = False
        For Each menuItem As MenuItem In MenuLanguage.MenuItems
            If menuItem.Tag = lang Then
                found = True
                menuItem.Checked = True
                Exit For
            End If
        Next
        If Not found Then
            MenuEnglish.Checked = True
        End If

        'Add any initialization after the InitializeComponent() call
        '==> Put your pre-execution steps here.
    End Sub

#Region "メインメニュー のクリック処理"
    '開く
    Private Sub MenuFileOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileOpen.Click
        OpenFileDialog.FileName = ""
        OpenFileDialog.Filter = "VisualBoyAdvance Battery File|*.sav|ProActionReplay SaveData File|*.xps|GameShark SnapShot File|*.sps"
        OpenFileDialog.ShowDialog()
        If OpenFileDialog.FileName <> "" Then
            loadFlag = OpenFileDialog.FilterIndex
            fname = OpenFileDialog.FileName
            LoadFile()
            Initializer()

            SetEnabled(True)
            Timer1.Enabled = True
            RcButtonSetting()
        End If
    End Sub

    'ファイルの読み込み処理
    Private Sub LoadFile()
        If loadFlag = 0 Then
            be = New EXE6SaveDataEditor(fname)
        Else
            be = New EXE6SaveDataEditor(fname, True)
        End If
        If be.CalcCheckSum <> be.CheckSumValue Then
            MsgBox("セーブデータが壊れています")
        End If
    End Sub

    'ファイルの読み込み
    Private Sub Initializer()

        LibGigaChipLB.Items.Clear()
        If (be.VersionInfo = "グレイガ") Then
            LibGigaChipLB.Items.AddRange(EXE6DataList.LibGigaChipNameListAtGrayga)
            MenuBeast.Text = "電脳獣グレイガチップデータ"
        ElseIf (be.VersionInfo = "ファルザー") Then
            LibGigaChipLB.Items.AddRange(EXE6DataList.LibGigaChipNameListAtFalther)
            MenuBeast.Text = "電脳獣ファルザーチップデータ"
        End If

        ClearDatabindings()
        SetDatabindings()
        For i As Integer = 0 To EXE6DataList.KeyItemList.Length - 1
            KeyItemLB.SetItemChecked(i, be.KeyItemsFlag(i))
        Next
        NaviCustCB.SelectedIndex = 0
        SubChipCB.SelectedIndex = 0
        ChipCB.SelectedIndex = 0
        RcCardList.SelectedIndex = 0
        ListBox1.BeginUpdate()
        FolderSelectCB.SelectedIndex = 0
        ReadFolder()
        ListBox1.EndUpdate()
        RcCountTB.Text = be.RemodelCardCount
        ReadRemodelCard() '改造カードをRcListに読み込む
        ReadMapValue()
        ReadLibrary()
    End Sub

    '名付け保存
    Private Sub MenuItem10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileSave.Click
        If be Is Nothing Then Exit Sub
        SaveFileDialog.FileName = ""
        'ロードフラグによってフィルタを設定
        Select Case loadFlag
            Case lFlag.VBA
                SaveFileDialog.Filter = "VisualBoyAdvance Battery File|*.sav"
            Case lFlag.PAR
                SaveFileDialog.Filter = "ProActionReplay SaveData File|*.xps"
            Case lFlag.GS
                SaveFileDialog.Filter = "GameShark SnapShot File|*.sps"
            Case Else
                Exit Sub
        End Select

        SaveFileDialog.ShowDialog()
        If SaveFileDialog.FileName <> "" Then
            SaveFile(SaveFileDialog.FileName)
        End If
    End Sub

    '上書き保存
    Private Sub MenuItem11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileSaveOverWrite.Click
        If be Is Nothing Then Exit Sub
        SaveFile(fname)
    End Sub

    'ファイルへ書き込む処理
    Private Sub SaveFile(ByVal FileName As String)

        CheckTextBox(ZennyTB, be.ZennyValue)
        CheckTextBox(BugFragsTB, be.BugPieceValue)
        CheckTextBox(ChipTB, be.ChipValue)
        ''サブチップ
        CheckTextBox(SubChipTB, be.SubChipValue) '存在フラグ
        CheckTextBox(MaxOfSubchipNumTB, be.MaxOfSubChipValue) '最大枚数
        ''改造カード
        CheckTextBox(RcCapacityTB, be.RemodelCardCapacity)
        ''拡張メモリ
        CheckTextBox(ExpandMemoryTB, be.ExtendMemoryValue)
        ''時間
        CheckTextBox(HourTB, be.HourValue)
        CheckTextBox(MinuteTB, be.MinuteValue)
        CheckTextBox(SecondTB, be.SecondValue)
        CheckTextBox(MsecondTB, be.MsecondValue)

        CheckTextBox(FolderValueTB, be.FolderValue)
        ''ナビカスタマイザー
        CheckTextBox(NaviCustTB, be.NaviCusValue)


        WriteRemodelCard() '改造カードを書き込む
        be.SetCheckSum()
        If loadFlag = 2 Then
            be.WriteFile(FileName, True)
        Else
            be.WriteFile(FileName)
        End If
    End Sub

    '
    Private Sub CheckTextBox(ByVal _TextBox As TextBox, ByRef SourceValue As Integer)
        If IsNumeric(_TextBox.Text) Then
            SourceValue = Val(_TextBox.Text)
        Else
            _TextBox.Text = SourceValue
        End If
    End Sub

    'フォルダー→ファイルへ保存
    Private Sub MenuItem27_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFolderSave.Click
        SaveFileDialog.FileName = ""
        SaveFileDialog.Filter = "folder file|*.dmp"
        SaveFileDialog.ShowDialog()
        If SaveFileDialog.FileName <> "" Then
            Dim fs As New System.IO.FileStream(SaveFileDialog.FileName,
                System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim bw As New System.IO.BinaryWriter(fs)
            For i As Integer = 0 To 29
                bw.Write(CType(be.ReadHalfWord(&H2178 + (i * 2) + FolderSelectCB.SelectedIndex * &H3C + be.BaseAddr), Short))
            Next
            bw.Close()
            fs.Close()
        End If
    End Sub

    'フォルダー→ファイルから読み込み
    Private Sub MenuFolderLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFolderLoad.Click
        OpenFileDialog.FileName = ""
        OpenFileDialog.Filter = "folder file|*.dmp"
        OpenFileDialog.ShowDialog()
        If OpenFileDialog.FileName <> "" Then
            Dim fs As New System.IO.FileStream(OpenFileDialog.FileName,
                System.IO.FileMode.Open, System.IO.FileAccess.Read)
            Dim br As New System.IO.BinaryReader(fs)
            For i As Integer = 0 To 29
                be.WriteHalfWord(&H2178 + (i * 2) + FolderSelectCB.SelectedIndex * &H3C + be.BaseAddr, br.ReadInt16)
            Next
            br.Close()
            fs.Close()
        End If
        ReadFolder()
    End Sub

    '閉じる
    Private Sub MenuFileClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileClose.Click
        SetEnabled(False)
        StatusDisplay.Text = ""
        Timer1.Enabled = False
    End Sub

    '終了
    Private Sub MenuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuExit.Click
        Me.Close()
    End Sub
#End Region

#Region "メインメニュー 編集 のクリック処理"
    'ライブラリ全開→全て
    Private Sub MenuItem16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem16.Click
        MenuItem18_Click(MenuItem18, New System.EventArgs)
        MenuItem19_Click(MenuItem19, New System.EventArgs)
        MenuItem17_Click(MenuItem17, New System.EventArgs)
        MenuItem20_Click(MenuItem20, New System.EventArgs)
        MenuItem21_Click(MenuItem21, New System.EventArgs)
    End Sub

    'ライブラリ全開→メガクラスのみ
    Private Sub MenuItem18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem18.Click
        For i As Integer = 0 To LibMegaChipLB.Items.Count - 1
            LibMegaChipLB.SetItemChecked(i, True)
        Next
        LibMegaChipLB_SelectedIndexChanged(LibMegaChipLB, New System.EventArgs)
    End Sub

    'ライブラリ全開→ギガクラスのみ
    Private Sub MenuItem19_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem19.Click
        For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
            LibGigaChipLB.SetItemChecked(i, True)
        Next
        LibGigaChipLB_SelectedIndexChanged(LibGigaChipLB, New System.EventArgs)
    End Sub

    'ライブラリ全開→スタンダードのみ
    Private Sub MenuItem17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem17.Click
        For i As Integer = 0 To LibraryLB.Items.Count - 1
            LibraryLB.SetItemChecked(i, True)
        Next
        LIbraryLB_SelectedIndexChanged(LibraryLB, New System.EventArgs)
    End Sub

    'ライブラリ全開→シークレットのみ
    Private Sub MenuItem20_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem20.Click
        For i As Integer = 0 To LibSecretChipLB.Items.Count - 1
            LibSecretChipLB.SetItemChecked(i, True)
        Next
        LibSeacretChipLB_SelectedIndexChanged(LibSecretChipLB, New System.EventArgs)
    End Sub

    'ライブラリ全開→Ｐ．Ａ．のみ
    Private Sub MenuItem21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem21.Click
        For i As Integer = 0 To LibPaLB.Items.Count - 1
            LibPaLB.SetItemChecked(i, True)
        Next
        LibPaLB_SelectedIndexChanged(LibPaLB, New System.EventArgs)
    End Sub

    'ダブルビースト→データを追加
    Private Sub MenuItem7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem7.Click
        If Not be Is Nothing Then be.SetDoubleBeast()
    End Sub

    'ダブルビースト→データを削除
    Private Sub MenuItem8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem8.Click
        If Not be Is Nothing Then be.ResetDoubleBeast()
    End Sub

    Private Sub MenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem1.Click
        If Not be Is Nothing Then be.SetBeast()
    End Sub

    Private Sub MenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem2.Click
        If Not be Is Nothing Then be.ResetBeast()
    End Sub

    '全て×99
    Private Sub MenuItem9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuChip.Click
        If Not be Is Nothing Then
            be.SetAllChip()
            ChipTB.Text = be.ChipValue
        End If
    End Sub

    'サブチップ全て８
    Private Sub MenuItem13_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuSubChip.Click
        For i As Integer = 0 To 7
            be.SubChipExistFlag(i) = True
            be.SubChipValue(i) = 8
            be.MaxOfSubChipValue = 8
        Next
        SubChipTB.Text = be.SubChipValue
        SubChipCkB.Checked = True
        MaxOfSubchipNumTB.Text = 8
    End Sub

    'ナビカス全て９
    Private Sub MenuItem14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuNaviCust.Click
        For i As Integer = 0 To EXE6DataList.NaviCusName.Length - 1
            be.NaviCusExistFlag(i) = True
            be.NaviCusValue(i) = 9
        Next
        NaviCustTB.Text = be.NaviCusValue
        NaviCustCkB.Checked = True
    End Sub

    'キーアイテム全て
    Private Sub MenuItem15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuKeyItem.Click
        For i As Integer = 0 To KeyItemLB.Items.Count - 1
            KeyItemLB.SetItemChecked(i, True)
        Next
        KeyItemLB_ItemCheck()
    End Sub
#End Region

#Region " ライブラリー関連の処理 "
    '
    Private Sub LIbraryLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibraryLB.SelectedIndexChanged
        For i As Integer = 0 To LibraryLB.Items.Count - 1
            be.LibraryFlag(EXE6DataList.LibStandardChipFlagList, i) = LibraryLB.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub LibSeacretChipLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibSecretChipLB.SelectedIndexChanged
        For i As Integer = 0 To LibSecretChipLB.Items.Count - 1
            be.LibraryFlag(EXE6DataList.LibSeacretChipFlagList, i) = LibSecretChipLB.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub LibGigaChipLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibGigaChipLB.SelectedIndexChanged

        If (be.VersionInfo = "グレイガ") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtGrayga, i) = LibGigaChipLB.GetItemChecked(i)
            Next
        ElseIf (be.VersionInfo = "ファルザー") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtFalther, i) = LibGigaChipLB.GetItemChecked(i)
            Next
        End If

    End Sub

    '
    Private Sub LibMegaChipLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibMegaChipLB.SelectedIndexChanged
        For i As Integer = 0 To LibMegaChipLB.Items.Count - 1
            be.LibraryFlag(EXE6DataList.LibMegaChipFlagList, i) = LibMegaChipLB.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub LibPaLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibPaLB.SelectedIndexChanged
        For i As Integer = 0 To LibPaLB.Items.Count - 1
            be.LibraryFlag(EXE6DataList.LibPaFlagList, i) = LibPaLB.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub ReadLibrary()
        For i As Integer = 0 To LibraryLB.Items.Count - 1
            LibraryLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibStandardChipFlagList, i))
        Next
        For i As Integer = 0 To LibSecretChipLB.Items.Count - 1
            LibSecretChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibSeacretChipFlagList, i))
        Next
        For i As Integer = 0 To LibMegaChipLB.Items.Count - 1
            LibMegaChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibMegaChipFlagList, i))
        Next
        If (be.VersionInfo = "グレイガ") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                LibGigaChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtGrayga, i))
            Next

        ElseIf (be.VersionInfo = "ファルザー") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                LibGigaChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtFalther, i))
            Next
        End If


        For i As Integer = 0 To LibPaLB.Items.Count - 1
            LibPaLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibPaFlagList, i))
        Next
    End Sub
#End Region

#Region " 改造カード "
    '改造カードの追加処理
    Private Sub RcAddButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RcAddButton.Click
        Dim bufIndex As Integer = RcList.SelectedIndex
        RcList.Items.Insert(RcList.SelectedIndex + 1, RcCardList.Items(RcCardList.SelectedIndex))
        RcList.SelectedIndex = bufIndex + 1
        RcCountTB.Text = RcList.Items.Count
        be.RemodelCardCount = RcList.Items.Count
        WriteRemodelCard()
        RcButtonSetting()
        RcDisplayExistFlagCkB.Checked = True
    End Sub

    '改造カードの削除処理
    Private Sub RcRemoveButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RcRemoveButton.Click
        Dim bufIndex As Integer = RcList.SelectedIndex
        RcList.Items.RemoveAt(RcList.SelectedIndex)

        If bufIndex = RcList.Items.Count Then
            RcList.SelectedIndex = bufIndex - 1
        Else
            RcList.SelectedIndex = bufIndex
        End If
        RcCountTB.Text = RcList.Items.Count
        be.RemodelCardCount = RcList.Items.Count
        WriteRemodelCard()
        RcButtonSetting()
    End Sub

    '改造カードが規定数のときボタンのEnabledを設定
    Private Sub RcButtonSetting()
        If RcList.Items.Count >= 32 Then
            RcAddButton.Enabled = False
        Else
            RcAddButton.Enabled = True

        End If
        If RcList.Items.Count = 0 Then
            RcRemoveButton.Enabled = False
        Else
            RcRemoveButton.Enabled = True
        End If
    End Sub

    '
    Private Sub WriteRemodelCard()
        For i As Integer = 0 To be.RemodelCardCount - 1
            be.RemodelCard(i) = Val("&H" & RcList.Items(i).Substring(0, 2))
            be.SetRemodelCardLoadFlag(be.RemodelCard(i) - 1)
            be.RemodelSwitch(i) = RcList.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub ReadRemodelCard()
        RcList.Items.Clear()
        For i As Integer = 0 To be.RemodelCardCount - 1
            RcList.Items.Add(RcCardList.Items((be.RemodelCard(i) - 1)))
            RcList.SetItemChecked(i, be.RemodelSwitch(i))
        Next
        If be.RemodelCardCount > 0 Then RcList.SelectedIndex = 0
    End Sub
#End Region

#Region " フォルダ関連の処理 "
    'フォルダへチップを追加する処理
    Private Sub AddChipButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddChipButton.Click
        Dim index As Integer = ListBox1.SelectedIndex
        ListBox1.Items.RemoveAt(index)
        ListBox1.Items.Insert(index, ListBox2.Items(ListBox2.SelectedIndex))
        ListBox1.SelectedIndex = index
        be.WriteHalfWord(&H2178 + (index * 2) + FolderSelectCB.SelectedIndex * &H3C + be.BaseAddr, EXE6DataList.ChipCodeList(ListBox2.SelectedIndex))
    End Sub

    '
    Private Sub ReadFolder()
        ListBox1.Items.Clear()
        For i As Integer = 0 To 29
            Dim index As Integer
            For j As Integer = 0 To EXE6DataList.ChipCodeList.Length - 1
                If EXE6DataList.ChipCodeList(j) = be.ReadHalfWord(&H2178 + (i * 2) + FolderSelectCB.SelectedIndex * &H3C + be.BaseAddr) Then
                    index = j
                End If

            Next
            ListBox1.Items.Add(EXE6DataList.ChipNameList(index))
        Next
        ListBox1.SelectedIndex = 0
    End Sub

    '
    Private Sub FolderSelectCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FolderSelectCB.SelectedIndexChanged
        ReadFolder()
    End Sub

    '補正ボタン
    Private Sub ChipRepairButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChipRepairButton.Click
        CheckChipNum()
    End Sub

    'チップ枚数補正
    Private Sub CheckChipNum()
        Dim maxchipNumber As Integer = 30 * FolderValueTB.Text - 1
        Dim chip(maxchipNumber) As Integer
        'チップを読み込む
        For i As Integer = 0 To maxchipNumber
            chip(i) = be.ReadHalfWord(&H2178 + (i * 2) + FolderSelectCB.SelectedIndex * &H3C + be.BaseAddr)
        Next
        Array.Sort(chip)
        Dim rankValue(maxchipNumber) As Integer
        Dim rank(maxchipNumber) As Integer
        Dim tmpVal As Integer = -1
        Dim index As Integer = -1
        For Each tmp As Integer In chip
            If tmpVal = tmp Then
                rank(index) += 1
            Else
                tmpVal = tmp
                index += 1
                rank(index) += 1
                rankValue(index) = Array.IndexOf(chip, tmp)
            End If
        Next

        'フォルダのチップ数が所持数を上回ったら補正
        For i As Integer = 0 To maxchipNumber
            If rank(i) = 0 Then Exit For
            If be.ReadByte(be.BaseAddr + EXE6DataList.chipAddrList(rankValue(i))) < rank(i) Then
                be.WriteByte(be.BaseAddr + EXE6DataList.chipAddrList(rankValue(i)), rank(i))
            End If
        Next
        ChipTB.Text = be.ChipValue
    End Sub
#End Region

    'チップ
    Private Sub ChipCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChipCB.SelectedIndexChanged
        be.ChipIndex = ChipCB.SelectedIndex
        ChipTB.Text = be.ChipValue
    End Sub

    'サブチップ
    Private Sub SubChipCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SubChipCB.SelectedIndexChanged
        be.SubChipIndex = SubChipCB.SelectedIndex
        SubChipTB.Text = be.SubChipValue
        SubChipCkB.Checked = be.SubChipExistFlag
    End Sub

    'ナビカス
    Private Sub NaviCusCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NaviCustCB.SelectedIndexChanged
        be.NaviCusIndex = NaviCustCB.SelectedIndex
        NaviCustTB.Text = be.NaviCusValue
    End Sub

    'キーアイテムの書き込み処理
    Private Sub KeyItemLB_ItemCheck()
        For i As Integer = 0 To EXE6DataList.KeyItemList.Length - 1
            be.KeyItemsFlag(i) = KeyItemLB.GetItemChecked(i)
        Next
    End Sub

    'キーアイテムの書き込み
    Private Sub KeyItemLB_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyItemLB.Leave
        KeyItemLB_ItemCheck()
    End Sub

    'マップリストから読み込む
    Private Sub ReadMapValue()
        MapListCB.SelectedIndex = Array.IndexOf(EXE6DataList.MapListCode, be.MapCodeValue)
    End Sub

    'マップのコンボボックスが変更されたら書き込む
    Private Sub MapListCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MapListCB.SelectedIndexChanged
        be.MapCodeValue = EXE6DataList.MapListCode(MapListCB.SelectedIndex)
    End Sub

    'バージョン情報のフォームを表示
    Private Sub MenuVersionInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuVersionInfo.Click
        Dim f As New VerInfo
        f.ShowDialog()
        f.Dispose()
    End Sub

    '検索フォームを表示する
    Private Sub MenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem3.Click
        Dim f As New FindStringForm
        'フォームの
        If FindCM.SourceControl.GetType.Equals(GetType(ComboBox)) Then
            f.Show(CType(FindCM.SourceControl, ComboBox))
        ElseIf FindCM.SourceControl.GetType.Equals(GetType(ListBox)) Then
            f.Show(CType(FindCM.SourceControl, ListBox))
        ElseIf FindCM.SourceControl.GetType.Equals(GetType(CheckedListBox)) Then
            f.Show(CType(FindCM.SourceControl, ListBox))
        End If
    End Sub

    'DataBindingsを追加
    Private Sub SetDatabindings()
        ZennyTB.DataBindings.Add("Text", be, "ZennyValue") 'ZENNY
        BugFragsTB.DataBindings.Add("Text", be, "BugPieceValue") 'BUGPIECE
        ChipTB.DataBindings.Add("Text", be, "ChipValue") 'チップ
        'サブチップ
        SubChipTB.DataBindings.Add("Text", be, "SubChipValue") '枚数
        SubChipCkB.DataBindings.Add("Checked", be, "SubChipExistFlag") '存在フラグ
        MaxOfSubchipNumTB.DataBindings.Add("Text", be, "MaxOfSubChipValue") '最大枚数
        '改造カード
        RcDisplayExistFlagCkB.DataBindings.Add("Checked", be, "RemodelCardDisplayExistFlag")
        RcCapacityTB.DataBindings.Add("Text", be, "RemodelCardCapacity")
        '拡張メモリ
        ExpandMemoryTB.DataBindings.Add("Text", be, "ExtendMemoryValue")
        ExpandMemoryCkB.DataBindings.Add("Checked", be, "ExtendMemoryExistFlag")
        '時間
        HourTB.DataBindings.Add("Text", be, "HourValue")
        MinuteTB.DataBindings.Add("Text", be, "MinuteValue")
        SecondTB.DataBindings.Add("Text", be, "SecondValue")
        MsecondTB.DataBindings.Add("Text", be, "MsecondValue")
        FolderValueTB.DataBindings.Add("Text", be, "FolderValue")
        'ナビカスタマイザー
        NaviCustTB.DataBindings.Add("Text", be, "NaviCusValue")
        NaviCustCkB.DataBindings.Add("Checked", be, "NaviCusExistFlag")
        NaviCustExistFlagCkB.DataBindings.Add("Checked", be, "NaviCusDisplayExistFlag")
        CompressCommandCkB.DataBindings.Add("Checked", be, "CompressCommandFlag")
    End Sub

    'DataBindingsを削除
    Private Sub ClearDatabindings()
        ZennyTB.DataBindings.Clear() 'ZENNY
        BugFragsTB.DataBindings.Clear() 'BUGPIECE
        ChipTB.DataBindings.Clear() 'チップ
        'サブチップ
        SubChipTB.DataBindings.Clear() '枚数
        SubChipCkB.DataBindings.Clear() '存在フラグ
        MaxOfSubchipNumTB.DataBindings.Clear() '最大枚数
        '改造カード
        RcDisplayExistFlagCkB.DataBindings.Clear()
        RcCapacityTB.DataBindings.Clear()
        '拡張メモリ
        ExpandMemoryTB.DataBindings.Clear()
        ExpandMemoryCkB.DataBindings.Clear()
        '時間
        HourTB.DataBindings.Clear()
        MinuteTB.DataBindings.Clear()
        SecondTB.DataBindings.Clear()
        MsecondTB.DataBindings.Clear()
        FolderValueTB.DataBindings.Clear()
        'ナビカスタマイザー
        NaviCustTB.DataBindings.Clear()
        NaviCustCkB.DataBindings.Clear()
        NaviCustExistFlagCkB.DataBindings.Clear()
        CompressCommandCkB.DataBindings.Clear()
    End Sub

    'Enabledをまとめて指定
    Private Sub SetEnabled(ByVal enable As Boolean)
        Me.enabled = enable

        normalGB.Enabled = enable
        ChipGB.Enabled = enable
        SubChipGB.Enabled = enable
        NaviCustGB.Enabled = enable
        RemodelCardGB.Enabled = enable
        KeyItemGB.Enabled = enable
        MenuEdit.Enabled = enable   '編集
        MenuFolder.Enabled = enable 'ファイル->フォルダ
        MenuFileSave.Enabled = enable 'ファイル->名付け保存
        MenuFileSaveOverWrite.Enabled = enable 'ファイル->上書き保存
        MenuFileClose.Enabled = enable 'ファイル->閉じる

        MapListGB.Enabled = enable
        FolderGB.Enabled = enable
        StandardChipGB.Enabled = enable
        MegaChipGB.Enabled = enable
        GigaChipGB.Enabled = enable
        SecretChipGB.Enabled = enable
        PaGB.Enabled = enable
    End Sub

    'セーブデータの状態を定期的に更新する
    Private Sub Timer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If be Is Nothing Then Exit Sub
        StatusDisplay.Text = be.ToString
        LoadFileName.Text = fname
        Select Case loadFlag
            Case 1
                StatusDisplay.Text &= "ファイルタイプ        :VisualBoyAdvance Battery File"
            Case 2
                StatusDisplay.Text &= "ファイルタイプ        :ProActionReplay SaveData File"
            Case 3
                StatusDisplay.Text &= "ファイルタイプ        :GameShark SnapShot File"
        End Select

    End Sub

    'フォームを閉じるときの処理
    Private Sub MainForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        'データが変更されているのなら警告する
        If Not be Is Nothing AndAlso be.CheckSumValue <> be.CalcCheckSum Then
            If MsgBoxResult.No = MsgBox("変更が保存されていません。" & vbNewLine & "本当に終了しますか", MsgBoxStyle.YesNo, "確認") Then
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        RcCardList.Items.AddRange(EXE6DataList.RemodelCardList)
        NaviCustCB.Items.AddRange(EXE6DataList.NaviCusName)
        KeyItemLB.Items.AddRange(EXE6DataList.KeyItemList)
        ChipCB.Items.AddRange(EXE6DataList.ChipNameList)
        ListBox2.Items.AddRange(EXE6DataList.ChipNameList)
        ListBox2.SelectedIndex = 0
        MapListCB.Items.AddRange(EXE6DataList.MapListName)

        'ライブラリのチップ名を読み込み
        LibraryLB.Items.AddRange(EXE6DataList.LibStandardChipNameList)
        LibSecretChipLB.Items.AddRange(EXE6DataList.LibSeacretChipNameList)
        LibMegaChipLB.Items.AddRange(EXE6DataList.LibMegaChipNameList)
        LibPaLB.Items.AddRange(EXE6DataList.LibPaNameList)
    End Sub

    Private Sub UpdateLanguage(sender As Object, e As EventArgs) Handles MenuJapanese.Click, MenuEnglish.Click
        Me.SuspendLayout()
        'Get menu item and culture name.
        Dim newMenuItem As MenuItem = sender
        Dim cultureName As String = newMenuItem.Tag

        'Save new language in settings.
        My.Settings.Language = cultureName

        'Uncheck all language items, then check this one.
        For Each otherItem As MenuItem In newMenuItem.Parent.MenuItems
            otherItem.Checked = False
        Next
        newMenuItem.Checked = True

        'Set the new UI culture.
        Dim cultureInfo As CultureInfo = New CultureInfo(cultureName)
        Thread.CurrentThread.CurrentUICulture = cultureInfo

        Dim resources As ComponentResourceManager = New ComponentResourceManager(Me.GetType())

        'Add all top-level controls and menu items to the collection.
        Dim controls As List(Of Control) = New List(Of Control)()
        For Each c As Control In Me.Controls
            controls.Add(c)
        Next c

        'Iterate through all controls, and update their text.
        Dim index As Integer = 0
        While index < controls.Count
            Dim control As Control = controls.Item(index)
            resources.ApplyResources(control, control.Name, cultureInfo)

            For Each c As Control In control.Controls
                controls.Add(c)
            Next c

            index += 1
        End While

        'Iterate through all menu items, and update their text.
        For Each fieldInfo As FieldInfo In Me.GetType().GetFields(BindingFlags.Instance Or BindingFlags.NonPublic)
            If fieldInfo.FieldType Is GetType(MenuItem) Then
                Dim name As String = fieldInfo.Name
                Dim nameStart As Integer = 0
                While name(nameStart) = "_"
                    nameStart += 1
                End While
                name = name.Substring(nameStart)

                resources.ApplyResources(fieldInfo.GetValue(Me), name, cultureInfo)
            End If
        Next

        For i As Integer = 0 To SubChipCB.Items.Count - 1
            Dim name As String = NameOf(SubChipCB) + ".Items" + If(i = 0, "", i.ToString())
            SubChipCB.Items(i) = resources.GetString(name)
        Next

        SetEnabled(Me.enabled)

        Me.ResumeLayout()
    End Sub
End Class