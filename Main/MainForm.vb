Imports EXE6DataList

Public Class MainForm
    Private fname As String
    Private be As EXE6SaveDataEditor

    Private loadFlag As Integer = lFlag.NONE

    '���[�h�t���O�񋓑�
    Enum lFlag
        NONE = 0    '�����[�h
        VBA = 1     'VisualBoyAdvance Battery File
        PAR = 2     'ProActionReplay Save Data File
        GS = 3      'GameShark SnapShot File
    End Enum

#Region "���C�����j���[ �̃N���b�N����"
    '�J��
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

    '�t�@�C���̓ǂݍ��ݏ���
    Private Sub LoadFile()
        If loadFlag = 0 Then
            be = New EXE6SaveDataEditor(fname)
        Else
            be = New EXE6SaveDataEditor(fname, True)
        End If
        If be.CalcCheckSum <> be.CheckSumValue Then
            MsgBox("�Z�[�u�f�[�^�����Ă��܂�")
        End If
    End Sub

    '�t�@�C���̓ǂݍ���
    Private Sub Initializer()

        LibGigaChipLB.Items.Clear()
        If (be.VersionInfo = "�O���C�K") Then
            LibGigaChipLB.Items.AddRange(EXE6DataList.LibGigaChipNameListAtGrayga)
            MenuBeast.Text = "�d�]�b�O���C�K�`�b�v�f�[�^"
        ElseIf (be.VersionInfo = "�t�@���U�[") Then
            LibGigaChipLB.Items.AddRange(EXE6DataList.LibGigaChipNameListAtFalther)
            MenuBeast.Text = "�d�]�b�t�@���U�[�`�b�v�f�[�^"
        End If

        ClearDatabindings()
        SetDatabindings()
        For i As Integer = 0 To EXE6DataList.KeyItemList.Length - 1
            KeyItemLB.SetItemChecked(i, be.KeyItemsFlag(i))
        Next
        NaviCusCB.SelectedIndex = 0
        SubChipCB.SelectedIndex = 0
        ChipCB.SelectedIndex = 0
        RcCardList.SelectedIndex = 0
        ListBox1.BeginUpdate()
        FolderSelectCB.SelectedIndex = 0
        ReadFolder()
        ListBox1.EndUpdate()
        RcCountTB.Text = be.RemodelCardCount
        ReadRemodelCard() '�����J�[�h��RcList�ɓǂݍ���
        ReadMapValue()
        ReadLibrary()
    End Sub

    '���t���ۑ�
    Private Sub MenuItem10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileSave.Click
        If be Is Nothing Then Exit Sub
        SaveFileDialog.FileName = ""
        '���[�h�t���O�ɂ���ăt�B���^��ݒ�
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

    '�㏑���ۑ�
    Private Sub MenuItem11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileSaveOverWrite.Click
        If be Is Nothing Then Exit Sub
        SaveFile(fname)
    End Sub

    '�t�@�C���֏������ޏ���
    Private Sub SaveFile(ByVal FileName As String)

        CheckTextBox(ZennyTB, be.ZennyValue)
        CheckTextBox(BugPieceTB, be.BugPieceValue)
        CheckTextBox(ChipTB, be.ChipValue)
        ''�T�u�`�b�v
        CheckTextBox(SubChipTB, be.SubChipValue) '���݃t���O
        CheckTextBox(MaxOfSubchipNumTB, be.MaxOfSubChipValue) '�ő喇��
        ''�����J�[�h
        CheckTextBox(RcCapacityTB, be.RemodelCardCapacity)
        ''�g��������
        CheckTextBox(ExtendMemoryTB, be.ExtendMemoryValue)
        ''����
        CheckTextBox(HourTB, be.HourValue)
        CheckTextBox(MinuteTB, be.MinuteValue)
        CheckTextBox(SecondTB, be.SecondValue)
        CheckTextBox(MsecondTB, be.MsecondValue)

        CheckTextBox(FolderValueTB, be.FolderValue)
        ''�i�r�J�X�^�}�C�U�[
        CheckTextBox(NaviCusTB, be.NaviCusValue)


        WriteRemodelCard() '�����J�[�h����������
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

    '�t�H���_�[���t�@�C���֕ۑ�
    Private Sub MenuItem27_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFolderSave.Click
        SaveFileDialog.FileName = ""
        SaveFileDialog.Filter = "folder file|*.dmp"
        SaveFileDialog.ShowDialog()
        If SaveFileDialog.FileName <> "" Then
            Dim fs As New System.IO.FileStream(SaveFileDialog.FileName, _
                System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim bw As New System.IO.BinaryWriter(fs)
            For i As Integer = 0 To 29
                bw.Write(CType(be.ReadHalfWord(&H2178 + (i * 2) + FolderSelectCB.SelectedIndex * &H3C + be.BaseAddr), Short))
            Next
            bw.Close()
            fs.Close()
        End If
    End Sub

    '�t�H���_�[���t�@�C������ǂݍ���
    Private Sub MenuFolderLoad_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFolderLoad.Click
        OpenFileDialog.FileName = ""
        OpenFileDialog.Filter = "folder file|*.dmp"
        OpenFileDialog.ShowDialog()
        If OpenFileDialog.FileName <> "" Then
            Dim fs As New System.IO.FileStream(OpenFileDialog.FileName, _
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

    '����
    Private Sub MenuFileClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuFileClose.Click
        SetEnabled(False)
        StatusDisplay.Text = ""
        Timer1.Enabled = False
    End Sub

    '�I��
    Private Sub MenuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuExit.Click
        Me.Close()
    End Sub
#End Region

#Region "���C�����j���[ �ҏW �̃N���b�N����"
    '���C�u�����S�J���S��
    Private Sub MenuItem16_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem16.Click
        MenuItem18_Click(MenuItem18, New System.EventArgs)
        MenuItem19_Click(MenuItem19, New System.EventArgs)
        MenuItem17_Click(MenuItem17, New System.EventArgs)
        MenuItem20_Click(MenuItem20, New System.EventArgs)
        MenuItem21_Click(MenuItem21, New System.EventArgs)
    End Sub

    '���C�u�����S�J�����K�N���X�̂�
    Private Sub MenuItem18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem18.Click
        For i As Integer = 0 To LibMegaChipLB.Items.Count - 1
            LibMegaChipLB.SetItemChecked(i, True)
        Next
        LibMegaChipLB_SelectedIndexChanged(LibMegaChipLB, New System.EventArgs)
    End Sub

    '���C�u�����S�J���M�K�N���X�̂�
    Private Sub MenuItem19_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem19.Click
        For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
            LibGigaChipLB.SetItemChecked(i, True)
        Next
        LibGigaChipLB_SelectedIndexChanged(LibGigaChipLB, New System.EventArgs)
    End Sub

    '���C�u�����S�J���X�^���_�[�h�̂�
    Private Sub MenuItem17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem17.Click
        For i As Integer = 0 To LibraryLB.Items.Count - 1
            LibraryLB.SetItemChecked(i, True)
        Next
        LIbraryLB_SelectedIndexChanged(LibraryLB, New System.EventArgs)
    End Sub

    '���C�u�����S�J���V�[�N���b�g�̂�
    Private Sub MenuItem20_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem20.Click
        For i As Integer = 0 To LibSeacretChipLB.Items.Count - 1
            LibSeacretChipLB.SetItemChecked(i, True)
        Next
        LibSeacretChipLB_SelectedIndexChanged(LibSeacretChipLB, New System.EventArgs)
    End Sub

    '���C�u�����S�J���o�D�`�D�̂�
    Private Sub MenuItem21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem21.Click
        For i As Integer = 0 To LibPaLB.Items.Count - 1
            LibPaLB.SetItemChecked(i, True)
        Next
        LibPaLB_SelectedIndexChanged(LibPaLB, New System.EventArgs)
    End Sub

    '�_�u���r�[�X�g���f�[�^��ǉ�
    Private Sub MenuItem7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem7.Click
        If Not be Is Nothing Then be.SetDoubleBeast()
    End Sub

    '�_�u���r�[�X�g���f�[�^���폜
    Private Sub MenuItem8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem8.Click
        If Not be Is Nothing Then be.ResetDoubleBeast()
    End Sub

    Private Sub MenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem1.Click
        If Not be Is Nothing Then be.SetBeast()
    End Sub

    Private Sub MenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem2.Click
        If Not be Is Nothing Then be.ResetBeast()
    End Sub

    '�S�ā~99
    Private Sub MenuItem9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuChip.Click
        If Not be Is Nothing Then
            be.SetAllChip()
            ChipTB.Text = be.ChipValue
        End If
    End Sub

    '�T�u�`�b�v�S�ĂW
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

    '�i�r�J�X�S�ĂX
    Private Sub MenuItem14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuNaviCus.Click
        For i As Integer = 0 To EXE6DataList.NaviCusName.Length - 1
            be.NaviCusExistFlag(i) = True
            be.NaviCusValue(i) = 9
        Next
        NaviCusTB.Text = be.NaviCusValue
        NaviCusCkB.Checked = True
    End Sub

    '�L�[�A�C�e���S��
    Private Sub MenuItem15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuKeyItem.Click
        For i As Integer = 0 To KeyItemLB.Items.Count - 1
            KeyItemLB.SetItemChecked(i, True)
        Next
        KeyItemLB_ItemCheck()
    End Sub
#End Region

#Region " ���C�u�����[�֘A�̏��� "
    '
    Private Sub LIbraryLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibraryLB.SelectedIndexChanged
        For i As Integer = 0 To LibraryLB.Items.Count - 1
            be.LibraryFlag(EXE6DataList.LibStandardChipFlagList, i) = LibraryLB.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub LibSeacretChipLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibSeacretChipLB.SelectedIndexChanged
        For i As Integer = 0 To LibSeacretChipLB.Items.Count - 1
            be.LibraryFlag(EXE6DataList.LibSeacretChipFlagList, i) = LibSeacretChipLB.GetItemChecked(i)
        Next
    End Sub

    '
    Private Sub LibGigaChipLB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LibGigaChipLB.SelectedIndexChanged

        If (be.VersionInfo = "�O���C�K") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtGrayga, i) = LibGigaChipLB.GetItemChecked(i)
            Next
        ElseIf (be.VersionInfo = "�t�@���U�[") Then
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
        For i As Integer = 0 To LibSeacretChipLB.Items.Count - 1
            LibSeacretChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibSeacretChipFlagList, i))
        Next
        For i As Integer = 0 To LibMegaChipLB.Items.Count - 1
            LibMegaChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibMegaChipFlagList, i))
        Next
        If (be.VersionInfo = "�O���C�K") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                LibGigaChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtGrayga, i))
            Next

        ElseIf (be.VersionInfo = "�t�@���U�[") Then
            For i As Integer = 0 To LibGigaChipLB.Items.Count - 1
                LibGigaChipLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibGigaChipFlagListAtFalther, i))
            Next
        End If


        For i As Integer = 0 To LibPaLB.Items.Count - 1
            LibPaLB.SetItemChecked(i, be.LibraryFlag(EXE6DataList.LibPaFlagList, i))
        Next
    End Sub
#End Region

#Region " �����J�[�h "
    '�����J�[�h�̒ǉ�����
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

    '�����J�[�h�̍폜����
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

    '�����J�[�h���K�萔�̂Ƃ��{�^����Enabled��ݒ�
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

#Region " �t�H���_�֘A�̏��� "
    '�t�H���_�փ`�b�v��ǉ����鏈��
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

    '�␳�{�^��
    Private Sub ChipRepairButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChipRepairButton.Click
        CheckChipNum()
    End Sub

    '�`�b�v�����␳
    Private Sub CheckChipNum()
        Dim maxchipNumber As Integer = 30 * FolderValueTB.Text - 1
        Dim chip(maxchipNumber) As Integer
        '�`�b�v��ǂݍ���
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

        '�t�H���_�̃`�b�v��������������������␳
        For i As Integer = 0 To maxchipNumber
            If rank(i) = 0 Then Exit For
            If be.ReadByte(be.BaseAddr + EXE6DataList.chipAddrList(rankValue(i))) < rank(i) Then
                be.WriteByte(be.BaseAddr + EXE6DataList.chipAddrList(rankValue(i)), rank(i))
            End If
        Next
        ChipTB.Text = be.ChipValue
    End Sub
#End Region

    '�`�b�v
    Private Sub ChipCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChipCB.SelectedIndexChanged
        be.ChipIndex = ChipCB.SelectedIndex
        ChipTB.Text = be.ChipValue
    End Sub

    '�T�u�`�b�v
    Private Sub SubChipCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SubChipCB.SelectedIndexChanged
        be.SubChipIndex = SubChipCB.SelectedIndex
        SubChipTB.Text = be.SubChipValue
        SubChipCkB.Checked = be.SubChipExistFlag
    End Sub

    '�i�r�J�X
    Private Sub NaviCusCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NaviCusCB.SelectedIndexChanged
        be.NaviCusIndex = NaviCusCB.SelectedIndex
        NaviCusTB.Text = be.NaviCusValue
    End Sub

    '�L�[�A�C�e���̏������ݏ���
    Private Sub KeyItemLB_ItemCheck()
        For i As Integer = 0 To EXE6DataList.KeyItemList.Length - 1
            be.KeyItemsFlag(i) = KeyItemLB.GetItemChecked(i)
        Next
    End Sub

    '�L�[�A�C�e���̏�������
    Private Sub KeyItemLB_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles KeyItemLB.Leave
        KeyItemLB_ItemCheck()
    End Sub

    '�}�b�v���X�g����ǂݍ���
    Private Sub ReadMapValue()
        MapListCB.SelectedIndex = Array.IndexOf(EXE6DataList.MapListCode, be.MapCodeValue)
    End Sub

    '�}�b�v�̃R���{�{�b�N�X���ύX���ꂽ�珑������
    Private Sub MapListCB_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MapListCB.SelectedIndexChanged
        be.MapCodeValue = EXE6DataList.MapListCode(MapListCB.SelectedIndex)
    End Sub

    '�o�[�W�������̃t�H�[����\��
    Private Sub MenuVersionInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuVersionInfo.Click
        Dim f As New VerInfo
        f.ShowDialog()
        f.Dispose()
    End Sub

    '�����t�H�[����\������
    Private Sub MenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItem3.Click
        Dim f As New FindStringForm
        '�t�H�[����
        If FindCM.SourceControl.GetType.Equals(GetType(ComboBox)) Then
            f.Show(CType(FindCM.SourceControl, ComboBox))
        ElseIf FindCM.SourceControl.GetType.Equals(GetType(ListBox)) Then
            f.Show(CType(FindCM.SourceControl, ListBox))
        ElseIf FindCM.SourceControl.GetType.Equals(GetType(CheckedListBox)) Then
            f.Show(CType(FindCM.SourceControl, ListBox))
        End If
    End Sub

    'DataBindings��ǉ�
    Private Sub SetDatabindings()
        ZennyTB.DataBindings.Add("Text", be, "ZennyValue") 'ZENNY
        BugPieceTB.DataBindings.Add("Text", be, "BugPieceValue") 'BUGPIECE
        ChipTB.DataBindings.Add("Text", be, "ChipValue") '�`�b�v
        '�T�u�`�b�v
        SubChipTB.DataBindings.Add("Text", be, "SubChipValue") '����
        SubChipCkB.DataBindings.Add("Checked", be, "SubChipExistFlag") '���݃t���O
        MaxOfSubchipNumTB.DataBindings.Add("Text", be, "MaxOfSubChipValue") '�ő喇��
        '�����J�[�h
        RcDisplayExistFlagCkB.DataBindings.Add("Checked", be, "RemodelCardDisplayExistFlag")
        RcCapacityTB.DataBindings.Add("Text", be, "RemodelCardCapacity")
        '�g��������
        ExtendMemoryTB.DataBindings.Add("Text", be, "ExtendMemoryValue")
        ExtendMemoryCkB.DataBindings.Add("Checked", be, "ExtendMemoryExistFlag")
        '����
        HourTB.DataBindings.Add("Text", be, "HourValue")
        MinuteTB.DataBindings.Add("Text", be, "MinuteValue")
        SecondTB.DataBindings.Add("Text", be, "SecondValue")
        MsecondTB.DataBindings.Add("Text", be, "MsecondValue")
        FolderValueTB.DataBindings.Add("Text", be, "FolderValue")
        '�i�r�J�X�^�}�C�U�[
        NaviCusTB.DataBindings.Add("Text", be, "NaviCusValue")
        NaviCusCkB.DataBindings.Add("Checked", be, "NaviCusExistFlag")
        NaviCusExistFlagCkB.DataBindings.Add("Checked", be, "NaviCusDisplayExistFlag")
        CompressCommandCkB.DataBindings.Add("Checked", be, "CompressCommandFlag")
    End Sub

    'DataBindings���폜
    Private Sub ClearDatabindings()
        ZennyTB.DataBindings.Clear() 'ZENNY
        BugPieceTB.DataBindings.Clear() 'BUGPIECE
        ChipTB.DataBindings.Clear() '�`�b�v
        '�T�u�`�b�v
        SubChipTB.DataBindings.Clear() '����
        SubChipCkB.DataBindings.Clear() '���݃t���O
        MaxOfSubchipNumTB.DataBindings.Clear() '�ő喇��
        '�����J�[�h
        RcDisplayExistFlagCkB.DataBindings.Clear()
        RcCapacityTB.DataBindings.Clear()
        '�g��������
        ExtendMemoryTB.DataBindings.Clear()
        ExtendMemoryCkB.DataBindings.Clear()
        '����
        HourTB.DataBindings.Clear()
        MinuteTB.DataBindings.Clear()
        SecondTB.DataBindings.Clear()
        MsecondTB.DataBindings.Clear()
        FolderValueTB.DataBindings.Clear()
        '�i�r�J�X�^�}�C�U�[
        NaviCusTB.DataBindings.Clear()
        NaviCusCkB.DataBindings.Clear()
        NaviCusExistFlagCkB.DataBindings.Clear()
        CompressCommandCkB.DataBindings.Clear()
    End Sub

    'Enabled���܂Ƃ߂Ďw��
    Private Sub SetEnabled(ByVal enable As Boolean)
        nomalGB.Enabled = enable
        ChipGB.Enabled = enable
        SubChipGB.Enabled = enable
        NaviCusGB.Enabled = enable
        RemodelCardGB.Enabled = enable
        KeyItemGB.Enabled = enable
        MenuEdit.Enabled = enable   '�ҏW
        MenuFolder.Enabled = enable '�t�@�C��->�t�H���_
        MenuFileSave.Enabled = enable '�t�@�C��->���t���ۑ�
        MenuFileSaveOverWrite.Enabled = enable '�t�@�C��->�㏑���ۑ�
        MenuFileClose.Enabled = enable '�t�@�C��->����

        MapListGB.Enabled = enable
        FolderGB.Enabled = enable
        StandardChipGB.Enabled = enable
        MegaChipGB.Enabled = enable
        GigaChipGB.Enabled = enable
        SeacretChipGB.Enabled = enable
        PaGB.Enabled = enable
    End Sub

    '�Z�[�u�f�[�^�̏�Ԃ����I�ɍX�V����
    Private Sub Timer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If be Is Nothing Then Exit Sub
        StatusDisplay.Text = be.ToString
        LoadFileName.Text = fname
        Select Case loadFlag
            Case 1
                StatusDisplay.Text &= "�t�@�C���^�C�v        :VisualBoyAdvance Battery File"
            Case 2
                StatusDisplay.Text &= "�t�@�C���^�C�v        :ProActionReplay SaveData File"
            Case 3
                StatusDisplay.Text &= "�t�@�C���^�C�v        :GameShark SnapShot File"
        End Select

    End Sub

    '�t�H�[�������Ƃ��̏���
    Private Sub MainForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        '�f�[�^���ύX����Ă���̂Ȃ�x������
        If Not be Is Nothing AndAlso be.CheckSumValue <> be.CalcCheckSum Then
            If MsgBoxResult.No = MsgBox("�ύX���ۑ�����Ă��܂���B" & vbNewLine & "�{���ɏI�����܂���", MsgBoxStyle.YesNo, "�m�F") Then
                e.Cancel = True
            End If
        End If
    End Sub

    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        RcCardList.Items.AddRange(EXE6DataList.RemodelCardList)
        NaviCusCB.Items.AddRange(EXE6DataList.NaviCusName)
        KeyItemLB.Items.AddRange(EXE6DataList.KeyItemList)
        ChipCB.Items.AddRange(EXE6DataList.ChipNameList)
        ListBox2.Items.AddRange(EXE6DataList.ChipNameList)
        ListBox2.SelectedIndex = 0
        MapListCB.Items.AddRange(EXE6DataList.MapListName)

        '���C�u�����̃`�b�v����ǂݍ���
        LibraryLB.Items.AddRange(EXE6DataList.LibStandardChipNameList)
        LibSeacretChipLB.Items.AddRange(EXE6DataList.LibSeacretChipNameList)
        LibMegaChipLB.Items.AddRange(EXE6DataList.LibMegaChipNameList)
        LibPaLB.Items.AddRange(EXE6DataList.LibPaNameList)
    End Sub
End Class