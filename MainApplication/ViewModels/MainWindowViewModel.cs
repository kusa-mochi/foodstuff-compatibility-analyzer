using System;
using System.Collections.ObjectModel;
using System.Threading;

using Livet;
using Livet.Commands;

using MainApplication.Models;

namespace MainApplication.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        private DataCollector _collector = null;
        private DBManager _db = null;

        public void Initialize()
        {
            _collector = new DataCollector();
            _db = new DBManager();
        }


        #region StartId変更通知プロパティ
        private int _StartId = 0;

        public int StartId
        {
            get
            { return _StartId; }
            set
            { 
                if (_StartId == value)
                    return;
                _StartId = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region CurrentId変更通知プロパティ
        private int _CurrentId = 1;

        public int CurrentId
        {
            get
            { return _CurrentId; }
            set
            { 
                if (_CurrentId == value)
                    return;
                _CurrentId = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region CollectionSpeed変更通知プロパティ
        private float _CollectionSpeed = 0.0f;

        public float CollectionSpeed
        {
            get
            { return _CollectionSpeed; }
            set
            { 
                if (_CollectionSpeed == value)
                    return;
                _CollectionSpeed = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsCollecting変更通知プロパティ
        private bool _IsCollecting = false;

        public bool IsCollecting
        {
            get
            { return _IsCollecting; }
            set
            { 
                if (_IsCollecting == value)
                    return;
                _IsCollecting = value;
                IsNotCollecting = !_IsCollecting;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region IsNotCollecting変更通知プロパティ
        private bool _IsNotCollecting = true;

        public bool IsNotCollecting
        {
            get
            { return _IsNotCollecting; }
            set
            { 
                if (_IsNotCollecting == value)
                    return;
                _IsNotCollecting = value;
                RaisePropertyChanged();
            }
        }
        #endregion



        #region ResultData変更通知プロパティ
        private ObservableCollection<RecipeRecord> _ResultData = new ObservableCollection<RecipeRecord>();

        public ObservableCollection<RecipeRecord> ResultData
        {
            get
            { return _ResultData; }
            set
            { 
                if (_ResultData == value)
                    return;
                _ResultData = value;
                RaisePropertyChanged();
            }
        }
        #endregion



        #region StartCommand
        private ViewModelCommand _StartCommand;

        public ViewModelCommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new ViewModelCommand(Start);
                }
                return _StartCommand;
            }
        }

        public async void Start()
        {
            IsCollecting = true;

            cancelSrc = new CancellationTokenSource();
            var progress = new Progress<RecipeRecord>((r) =>
            {
                // データが取得できなかった場合
                if(r.Id == 0)
                {
                    // 画面上のIDだけ更新する。
                    CurrentId++;
                    return;
                }

                // データは取得できたが読み込めない形式だった場合
                if(r.Id < 0)
                {
                    // データベースにエラー履歴を残す。
                    _db.AddErrorRecord(-r.Id);

                    // 画面上のIDだけ更新する。
                    CurrentId = -r.Id;
                    return;
                }

                // DataGridにレコードを追加する処理。
                ResultData.Add(r);

                // DBにレコードを追加する処理。
                _db.AddRecord(r);

                CurrentId = r.Id;
            });

            try
            {
                await _collector.CollectRecordAsync(StartId, progress, cancelSrc.Token);
            }
            catch(OperationCanceledException)
            {
                IsCollecting = false;
                return;
            }

            IsCollecting = false;
        }
        #endregion


        #region CancelCommand
        /// <summary>
        /// 収集をキャンセルするためのもの
        /// </summary>
        private CancellationTokenSource cancelSrc;

        private ViewModelCommand _CancelCommand;

        public ViewModelCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new ViewModelCommand(Cancel);
                }
                return _CancelCommand;
            }
        }

        public void Cancel()
        {
            if (cancelSrc != null)
            {
                cancelSrc.Cancel();
            }
        }
        #endregion


        #region MakeDBCommand
        private ViewModelCommand _MakeDBCommand;

        public ViewModelCommand MakeDBCommand
        {
            get
            {
                if (_MakeDBCommand == null)
                {
                    _MakeDBCommand = new ViewModelCommand(MakeDB);
                }
                return _MakeDBCommand;
            }
        }

        public void MakeDB()
        {

        }
        #endregion

    }
}
