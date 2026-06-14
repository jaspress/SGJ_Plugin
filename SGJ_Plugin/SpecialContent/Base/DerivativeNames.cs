using System.Collections.Generic;

namespace SGJ_Plugin.SpecialContent.Base
{
    public static class DerivativeNames
    {
        private static readonly HashSet<string> ExactNames = new HashSet<string>
        {
            "九尾狐 指挥官",
            "九尾狐 狙击手",
            "九尾狐 战斗专家",
            "九尾狐 士兵",
            "快速反应部队 指挥官",
            "快速反应部队 机枪手",
            "快速反应部队 精准射手",
            "快速反应部队 医疗兵",
            "快速反应部队 突击队员",
            "快速反应部队 盾牌手",
            "快速反应部队 士兵",
            "精锐快反 指挥官",
            "精锐快反 工程师",
            "精锐快反 机枪手",
            "精锐快反 医疗兵",
            "精锐快反 士兵",
            "战术应对二部 指挥官",
            "战术应对二部 机枪手",
            "战术应对二部 工程师",
            "战术应对二部 医疗兵",
            "战术应对二部 士兵",
            "落锤特战A连 指挥官",
            "落锤特战A连 无畏战士",
            "落锤特战A连 医疗专家",
            "落锤特战A连 支援兵",
            "落锤特战A连 作战专家",
            "落锤特战A连 先锋",
            "落锤特战A连 士兵",
            "落锤特战B连 指挥官",
            "落锤特战B连 副指挥",
            "落锤特战B连 机枪手",
            "落锤特战B连 技术员",
            "落锤特战B连 毒气专家",
            "落锤特战B连 医疗兵",
            "落锤特战B连 士兵",
            "落锤特战B连3队 组长",
            "落锤特战B连3队 维修专家",
            "落锤特战B连3队 组员",
            "律法左手调查小队 执法官",
            "律法左手调查小队 助手",
            "律法左手调查小队 抓捕手",
            "律法左手调查小队 征召人员",
            "缴械与手铐",
            "对讲机",
            "九尾狐权限卡",
            "P90-NTF冲锋枪",
            "沙漠之鹰手枪",
            "m82a1狙击枪",
            "特殊护理医疗包",
            "肾上腺素注射器",
            "苏打水",
            "绿色夜视仪",
            "防毒面具",
        };

        private static readonly Dictionary<char, char> Replacements = new Dictionary<char, char>
        {
            ['官'] = '令',
            ['兵'] = '卒',
            ['员'] = '士',
            ['人'] = '员',
            ['者'] = '士',
            ['手'] = '员',
            ['师'] = '士',
            ['医'] = '师',
            ['生'] = '士',
            ['家'] = '士',
            ['长'] = '领',
            ['队'] = '组',
            ['组'] = '队',
            ['客'] = '士',
            ['鬼'] = '影',
            ['怪'] = '灵',
            ['兽'] = '物',
            ['魔'] = '魇',
            ['龙'] = '麟',
            ['卡'] = '证',
            ['器'] = '具',
            ['机'] = '器',
            ['枪'] = '铳',
            ['刀'] = '刃',
            ['药'] = '剂',
            ['水'] = '液',
            ['帽'] = '冠',
            ['包'] = '袋',
            ['服'] = '装',
            ['箱'] = '匣',
            ['书'] = '册',
            ['件'] = '卷',
            ['池'] = '芯',
            ['0'] = '8',
            ['1'] = '7',
            ['2'] = '5',
            ['3'] = '6',
            ['4'] = '9',
            ['5'] = '6',
            ['6'] = '8',
            ['7'] = '1',
            ['8'] = '3',
            ['9'] = '0',
            ['s'] = 'z',
            ['S'] = 'Z',
        };

        public static string ToDerivativeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            if (ExactNames.Contains(name))
                return name;

            char[] chars = name.ToCharArray();
            for (int i = chars.Length - 1; i >= 0; i--)
            {
                if (!Replacements.TryGetValue(chars[i], out char replacement))
                    continue;

                chars[i] = replacement;
                return new string(chars);
            }

            chars[chars.Length - 1] = chars[chars.Length - 1] == '甲' ? '乙' : '甲';
            return new string(chars);
        }
    }
}
