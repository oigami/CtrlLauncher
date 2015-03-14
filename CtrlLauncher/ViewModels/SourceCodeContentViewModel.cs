﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ICSharpCode.AvalonEdit.Highlighting;

using CtrlLauncher.Models;

namespace CtrlLauncher.ViewModels
{
    public class SourceCodeContentViewModel : ViewModel
    {
        #region SourceDirectoryEntry変更通知プロパティ
        private IEnumerable<FileEntryViewModel> _SourceDirectoryEntry;

        public IEnumerable<FileEntryViewModel> SourceDirectoryEntry
        {
            get
            { return _SourceDirectoryEntry; }
            set
            {
                if (_SourceDirectoryEntry == value)
                    return;
                _SourceDirectoryEntry = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SourceCode変更通知プロパティ
        private string _SourceCode;

        public string SourceCode
        {
            get
            { return _SourceCode; }
            set
            {
                if (_SourceCode == value)
                    return;
                _SourceCode = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region HighlightingDefinition変更通知プロパティ
        private IHighlightingDefinition _HighlightingDefinition;

        public IHighlightingDefinition HighlightingDefinition
        {
            get
            { return _HighlightingDefinition; }
            set
            {
                if (_HighlightingDefinition == value)
                    return;
                _HighlightingDefinition = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region FlyoutHeader変更通知プロパティ
        public string FlyoutHeader
        {
            get
            {
                return "ソースコードビューア" + (AppInfoViewModel == null ? "" : string.IsNullOrEmpty(AppInfoViewModel.AppSpec.Title) ? "" : " - " + AppInfoViewModel.AppSpec.Title);
            }
        }
        #endregion

        #region FileName変更通知プロパティ
        private string _FileName;

        public string FileName
        {
            get
            { return _FileName; }
            set
            { 
                if (_FileName == value)
                    return;
                _FileName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsLoading変更通知プロパティ
        private bool _IsLoading;

        public bool IsLoading
        {
            get
            { return _IsLoading; }
            set
            {
                if (_IsLoading == value)
                    return;
                _IsLoading = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ErrorMessage変更通知プロパティ
        private string _ErrorMessage;

        public string ErrorMessage
        {
            get
            { return _ErrorMessage; }
            set
            {
                if (_ErrorMessage == value)
                    return;
                _ErrorMessage = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsVisibleErrorMessage");
            }
        }
        #endregion

        #region IsVisibleErrorMessage変更通知プロパティ
        public bool IsVisibleErrorMessage { get { return !string.IsNullOrEmpty(ErrorMessage); } }
        #endregion

        #region AppInfoViewModel変更通知プロパティ
        private AppInfoViewModel _AppInfoViewModel;

        public AppInfoViewModel AppInfoViewModel
        {
            get
            { return _AppInfoViewModel; }
            set
            {
                _AppInfoViewModel = value;

                SourceDirectoryEntry = new[] { new FileEntryViewModel(value.SourceAbsolutePath, value.AppSpec.Title) };
                SourceCode = "";
                FileName = "";
                ErrorMessage = "左のツリーからコードファイルを選択してください";

                RaisePropertyChanged();
                RaisePropertyChanged("FlyoutHeader");
            }
        }
        #endregion

        public async void ItemSelected(FileEntryViewModel entry)
        {
            if (entry == null || entry.IsDirectory) return;

            FileName = entry.Name;
            SourceCode = "";
            ErrorMessage = "";
            
            try
            {
                var size = entry.Size;
                if (size > 1024 * 1024) // 1 MB より大きいファイルは読み込まない
                {
                    ErrorMessage = "1 MB より大きいファイルは表示できません";
                    return;
                }

                IsLoading = true;

                var def = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(entry.Path));
                HighlightingDefinition = def;

                using (var sr = new StreamReader(entry.Path))
                {
                    var sb = new StringBuilder((int)size);
                    while (!sr.EndOfStream)
                    {
                        var str = await sr.ReadLineAsync();
                        if (str.AsEnumerable().Contains('\0')) // バイナリ簡易判定
                        {
                            ErrorMessage = "(バイナリファイル)";
                            return;
                        }
                        sb.AppendLine(str);
                    }
                    SourceCode = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "エラー: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public SourceCodeContentViewModel()
        {
        }
    }
}