using System;
using System.Collections.Generic;
using System.Linq;

namespace TWLogAnalyzer.tw
{
    class TwExpManager
    {
        private List<(DateTime time, long value)> Exps;
        private DateTime NextWrapTime;

        public TwExpManager()
        {
            Exps = new List<(DateTime time, long value)>();
            NextWrapTime = DateTime.MinValue;
        }

        public void Reset()
        {
            Exps = new List<(DateTime time, long value)>();
            NextWrapTime = DateTime.MinValue;
        }

        public void Update(TwChatLog log)
        {
            lock (this)
            {
                // 前回データ削除時間から1時間以上経過していた場合は1時間以上前のデータをさくる
                if (NextWrapTime < log.TimeStamp)
                {
                    Exps.RemoveAll(item => item.time < NextWrapTime);
                    NextWrapTime = log.TimeStamp.AddMinutes(60);
                }

                Exps.Add((log.TimeStamp, log.Value));
            }
        }

        /// <summary>
        /// 過去n分（引数で指定）の平均分給を取得
        /// </summary>
        /// <param name="min">過去何分の分給データを取得するか</param>
        /// <returns></returns>
        public long getExpPerMin(int min)
        {
            lock (this)
            {
                DateTime from = DateTime.Now.AddMinutes(0 - min);
                return Exps.Where(item => item.time > from).Sum(item => item.value) / min;
            }
        }
    }
}
