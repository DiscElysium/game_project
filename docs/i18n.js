(() => {
'use strict';
const messages={
  "page.title": {
    "zh": "假面骰子 · Masked Liar’s Dice",
    "en": "Masked Dice · AI Bluffing & Mind Games"
  },
  "page.description": {
    "zh": "假面骰子：与三名 AI 对手展开吹牛骰子博弈。观察随手牌与局势生成的微表情描述，在概率与谎言之间做出判断。",
    "en": "Play Liar’s Dice against three AI opponents. Read AI-generated tells, weigh the odds, and decide whom to trust. Explore the design behind the game."
  },
  "page.ogTitle": {
    "zh": "假面骰子 · Masked Liar’s Dice",
    "en": "Masked Dice · AI Bluffing & Mind Games"
  },
  "page.ogDescription": {
    "zh": "AI 生成微表情。你决定相信谁。",
    "en": "AI writes the tells. You decide whom to trust."
  },
  "nav.skip": {
    "zh": "跳到主要内容",
    "en": "Skip to content"
  },
  "nav.home": {
    "zh": "假面骰子，返回顶部",
    "en": "Masked Dice, back to top"
  },
  "nav.brand": {
    "zh": "假面骰子<small>MASKED LIAR’S DICE</small>",
    "en": "Masked Dice<small>MASKED LIAR’S DICE</small>"
  },
  "nav.label": {
    "zh": "主要导航",
    "en": "Main navigation"
  },
  "nav.play": {
    "zh": "在线游玩",
    "en": "Play"
  },
  "nav.tells": {
    "zh": "AI 微表情",
    "en": "AI Tells"
  },
  "nav.mindgame": {
    "zh": "心理博弈",
    "en": "Mind Games"
  },
  "nav.rules": {
    "zh": "游戏规则",
    "en": "Rules"
  },
  "nav.creator": {
    "zh": "设计思考",
    "en": "Design"
  },
  "nav.tagline": {
    "zh": "独立游戏 / 一场信任实验",
    "en": "An indie game / A study in trust"
  },
  "nav.language": {
    "zh": "切换语言",
    "en": "Language"
  },
  "hero.eyebrow": {
    "zh": "AI 微表情 · 心理博弈",
    "en": "AI-GENERATED TELLS · MIND GAMES"
  },
  "hero.title": {
    "zh": "假面<br/>骰子<span aria-hidden=\"true\" class=\"title-period\">.</span>",
    "en": "Masked<br>Dice<span class=\"title-period\" aria-hidden=\"true\">.</span>"
  },
  "hero.subtitle": {
    "zh": "所有人都有底牌。<br/>没有人只有一张脸。",
    "en": "Everyone has a hand.<br>Everyone wears a mask."
  },
  "hero.description": {
    "zh": "坐进悬崖议庭里的赌局，与三名 AI 对手同桌。从一瞬的目光、一声迟疑里寻找线索，在下一次叫牌前，决定你相信谁。",
    "en": "Take a seat in a cliffside tribunal with three AI opponents. Read a glance, a hesitation, a change in tone—and decide whom to trust before the next bid."
  },
  "hero.play": {
    "zh": "立即游玩",
    "en": "Play now"
  },
  "hero.rules": {
    "zh": "了解游戏规则",
    "en": "Learn the rules"
  },
  "hero.frame": {
    "zh": "底牌 · 未知",
    "en": "HIDDEN HANDS · OPEN QUESTIONS"
  },
  "player.region": {
    "zh": "在线游玩假面骰子",
    "en": "Play Masked Dice"
  },
  "player.coverAlt": {
    "zh": "悬崖议庭里的三名假面对手，原生深棕色 1bit 游戏画面",
    "en": "Three masked opponents in the cliffside tribunal, rendered in native brown-and-paper 1bit pixels"
  },
  "player.standalone": {
    "zh": "独立窗口游玩 ↗",
    "en": "Open game in a new tab ↗"
  },
  "player.controls": {
    "zh": "<kbd>TAB</kbd> 观察／牌桌　<kbd>右键</kbd> 环顾　<kbd>F2</kbd> 画面　<kbd>F3</kbd> AI 记录与调参",
    "en": "<kbd>TAB</kbd> Observe / table　<kbd>RMB</kbd> Look　<kbd>F2</kbd> Picture　<kbd>F3</kbd> AI lab"
  },
  "facts.tagline": {
    "zh": "看不见底牌。<br/>看得见反应。",
    "en": "Hidden dice.<br>Visible reactions."
  },
  "facts.human": {
    "zh": "<strong>01</strong>人类玩家",
    "en": "<strong>01</strong>Human player"
  },
  "facts.ai": {
    "zh": "<strong>03</strong>AI 对手",
    "en": "<strong>03</strong>AI opponents"
  },
  "facts.dice": {
    "zh": "<strong>05</strong>每人初始骰子",
    "en": "<strong>05</strong>Starting dice each"
  },
  "tells.title": {
    "zh": "AI 的心理，<br/>藏在细节里。",
    "en": "The AI’s mind.<br>In the smallest details."
  },
  "tells.intro": {
    "zh": "每一轮，AI 根据手牌、当前叫牌和回合历史分析局势，生成具体的微表情描述。从目光到下颌，从手指到语气，反应随局势变化。",
    "en": "Each turn, the AI considers its hand, the current bid and the round’s history to write a specific tell: a glance, a tense jaw, a movement of the fingers. Reactions change with the situation."
  },
  "tells.clue": {
    "zh": "你得到的是一条线索，而非一句真假判决。",
    "en": "You get a clue, never a verdict."
  },
  "tells.1.label": {
    "zh": "[ 目光 ]",
    "en": "[ GAZE ]"
  },
  "tells.1.title": {
    "zh": "看向你，<br/>还是避开你？",
    "en": "Looking at you,<br>or looking away?"
  },
  "tells.1.quote": {
    "zh": "「他的视线从骰盅滑向你，<br/>又在与你对视前停住。」",
    "en": "“His gaze drifts from the cup toward you,<br>then stops just short of eye contact.”"
  },
  "tells.1.note": {
    "zh": "一次迟疑，也可能让你改变叫牌。",
    "en": "A moment of hesitation can change your next bid."
  },
  "tells.2.label": {
    "zh": "[ 下颌 ]",
    "en": "[ TENSION ]"
  },
  "tells.2.title": {
    "zh": "笑容里，<br/>有多少底气？",
    "en": "How much confidence<br>is behind that smile?"
  },
  "tells.2.quote": {
    "zh": "「嘴角短暂扬起，<br/>下颌却比刚才绷得更紧。」",
    "en": "“The corner of his mouth lifts for a moment,<br>but his jaw is tighter than before.”"
  },
  "tells.2.note": {
    "zh": "把反应放回叫牌历史里，再下判断。",
    "en": "Read the reaction in the context of earlier bids."
  },
  "tells.3.label": {
    "zh": "[ 姿态 ]",
    "en": "[ POSTURE ]"
  },
  "tells.3.title": {
    "zh": "不动声色，<br/>也是一种反应。",
    "en": "Stillness<br>is a reaction, too."
  },
  "tells.3.quote": {
    "zh": "「他把身体向后靠，<br/>手指却仍紧贴着骰盅边缘。」",
    "en": "“He leans back in his chair,<br>but his fingers stay pressed against the cup.”"
  },
  "tells.3.note": {
    "zh": "脸上的平静，不会让底牌变得透明。",
    "en": "A calm face does not reveal a hidden hand."
  },
  "tells.flow.1": {
    "zh": "手牌 + 叫牌历史",
    "en": "Hand + bid history"
  },
  "tells.flow.2": {
    "zh": "AI 分析局势",
    "en": "AI reads the situation"
  },
  "tells.flow.3": {
    "zh": "生成微表情描述",
    "en": "A tell is generated"
  },
  "tells.flow.4": {
    "zh": "你做出判断",
    "en": "You make the call"
  },
  "tells.examples": {
    "zh": "以上为描述示例",
    "en": "Illustrative descriptions"
  },
  "mind.title": {
    "zh": "你看见了迟疑。<br/>但他真的<br/>在撒谎吗？",
    "en": "You saw the hesitation.<br>But was it<br>a lie?"
  },
  "mind.intro": {
    "zh": "紧张不一定是心虚，自信也不一定有底气。观察反应，衡量叫牌，然后做出你的判断。",
    "en": "Nerves do not always mean a bluff. Confidence does not always mean a strong hand. Read the reaction, weigh the bid, then make your call."
  },
  "demo.heading": {
    "zh": "观察与判断",
    "en": "Observe & decide"
  },
  "demo.note": {
    "zh": "三组预设情境，演示规则。真实对局请直接游玩上方封面中的游戏。",
    "en": "Three preset situations explain the rules. For a real match, play directly in the cover above."
  },
  "demo.turn": {
    "zh": "当前叫牌 / 轮到你判断",
    "en": "CURRENT BID / YOUR CALL"
  },
  "demo.players": {
    "zh": "1 人类 · 3 AI",
    "en": "1 HUMAN · 3 AI"
  },
  "demo.justBid": {
    "zh": "对手刚刚叫牌",
    "en": "Your opponent just bid"
  },
  "demo.tellLabel": {
    "zh": "[ 观察到的微表情 ]",
    "en": "[ OBSERVED TELL ]"
  },
  "demo.trust": {
    "zh": "相信，继续叫牌",
    "en": "Trust it. Keep bidding."
  },
  "demo.challenge": {
    "zh": "质疑，打开骰盅",
    "en": "Call the bluff."
  },
  "demo.ordinary": {
    "zh": "全场普通骰子",
    "en": "Matching ordinary dice"
  },
  "demo.special": {
    "zh": "叫牌者的特殊骰子",
    "en": "Bidder’s matching special dice"
  },
  "demo.total": {
    "zh": "有效点数数量",
    "en": "Effective count"
  },
  "demo.next": {
    "zh": "换一个情境",
    "en": "Try another situation"
  },
  "demo.clue": {
    "zh": "表情是线索。底牌，才是答案。",
    "en": "The tell is a clue. The dice are the answer."
  },
  "rules.title": {
    "zh": "同一颗骰子，<br/>两种立场。",
    "en": "One die.<br>Two different values."
  },
  "rules.intro": {
    "zh": "每人初始持有 4 颗普通骰子和 1 颗特殊骰子。叫牌、质疑，再揭开骰盅。你的底牌，不等于我的筹码。",
    "en": "Each player starts with four ordinary dice and one special die. Bid, challenge, then lift the cups. Your hidden advantage is not mine."
  },
  "rules.1.title": {
    "zh": "叫牌",
    "en": "Bid"
  },
  "rules.1.body": {
    "zh": "宣称全场至少有 X 个 Y 点。你可以握有底牌，也可以虚张声势。新的叫牌必须提高数量，或在数量相同时提高点数。",
    "en": "Claim that the table holds at least X dice showing Y. You may have the dice to back it up—or you may be bluffing. A new bid must raise the quantity, or raise the face while keeping the same quantity."
  },
  "rules.2.title": {
    "zh": "质疑",
    "en": "Challenge"
  },
  "rules.2.body": {
    "zh": "不相信上一家的叫牌，就打开骰盅。实际有效数量达到叫牌数量，叫牌者胜；不足时，质疑者胜。",
    "en": "Doubt the last bid? Lift the cups. If the effective count meets or exceeds the bid, the bidder wins. If it falls short, the challenger wins."
  },
  "rules.3.title": {
    "zh": "特殊骰",
    "en": "The special die"
  },
  "rules.3.body": {
    "zh": "当叫牌者被质疑时，它符合叫牌点数的特殊骰子，每颗算 3 颗。其他人的特殊骰子不计数。同一颗骰子，立场决定价值。",
    "en": "When a bid is challenged, each matching special die held by the bidder counts as three. Everyone else’s special dice count as zero. The same die has a different value depending on who made the bid."
  },
  "rules.specialAlt": {
    "zh": "特殊 6 点骰子",
    "en": "Special die showing six"
  },
  "rules.advantage": {
    "zh": "你的底牌。<br/>别人的盲区。",
    "en": "Your hidden edge.<br>Their blind spot."
  },
  "rules.special.2.title": {
    "zh": "叫牌者的特殊骰子",
    "en": "The bidder’s special die"
  },
  "rules.special.2.body": {
    "zh": "点数匹配时，单颗计作三颗。",
    "en": "When its face matches the bid, one die counts as three."
  },
  "rules.special.3.title": {
    "zh": "其他人的特殊骰子",
    "en": "Everyone else’s special dice"
  },
  "rules.special.3.body": {
    "zh": "本次质疑中，不参与计数。",
    "en": "They do not count toward this challenge."
  },
  "abilities.label": {
    "zh": "叫牌之后 / 一次特殊行动",
    "en": "AFTER YOUR BID / ONE SPECIAL ACTION"
  },
  "abilities.title": {
    "zh": "改变信息，<br/>也改变局势。",
    "en": "Change the information.<br>Change the game."
  },
  "abilities.peek": {
    "zh": "透视 · 看见秘密",
    "en": "X-ray · See a secret"
  },
  "abilities.peekBody": {
    "zh": "让一名对手的骰盅透明 3 秒，观察普通骰子的点数，为接下来的判断积累信息。",
    "en": "Make one opponent’s cup transparent for three seconds. Read the ordinary dice before your next decision."
  },
  "abilities.steal": {
    "zh": "窃取 · 夺走底牌",
    "en": "Steal · Take an advantage"
  },
  "abilities.stealBody": {
    "zh": "猜中一名对手的特殊骰子点数，将它收入自己的手牌。猜错则没有转移。",
    "en": "Guess the face of an opponent’s special die. Guess correctly and it becomes yours. Guess wrong and nothing changes hands."
  },
  "world.title": {
    "zh": "坐进赌局。<br/>听骰子落定。",
    "en": "Take your seat.<br>Let the dice settle."
  },
  "world.intro": {
    "zh": "星盘、狐狸与羊角，三张不同的假面。三名对手守着各自的秘密。下一句叫牌，会从谁开始？",
    "en": "An astrolabe, a fox and a ram: three distinct masks. Three opponents, each with a secret. Who will make the next bid?"
  },
  "world.open": {
    "zh": "查看对局实机完整原图",
    "en": "Open the full native gameplay capture"
  },
  "world.alt": {
    "zh": "最新版游戏实机，保留原生深棕色与纸色像素抖动渲染",
    "en": "The latest in-game view of the tribunal, with native brown-and-paper pixel dithering"
  },
  "world.caption": {
    "zh": "游戏实机 / 对局画面",
    "en": "IN-GAME CAPTURE / AT THE TABLE"
  },
  "world.full": {
    "zh": "查看完整画面",
    "en": "View full capture"
  },
  "creator.title": {
    "zh": "知其然，<br/>也知其所以然。",
    "en": "Understand how.<br>Understand why."
  },
  "creator.lead": {
    "zh": "我对 AI 的理解，来自原理，也来自大量实践。理解它的运行逻辑，让我更能判断它能做什么、适合做什么，以及怎样把它放进一个有效的系统。",
    "en": "My understanding of AI comes from studying its principles and using it extensively. Knowing how it works helps me judge what it can do, where it fits, and how to build it into a system that works."
  },
  "creator.disciplines": {
    "zh": "概率建模 × 语言模型 × 交互设计",
    "en": "PROBABILITY × LANGUAGE MODELS × INTERACTION DESIGN"
  },
  "creator.1.step": {
    "zh": "01 / 理解问题",
    "en": "01 / UNDERSTAND THE PROBLEM"
  },
  "creator.1.title": {
    "zh": "“吹牛”的背后，也有数学。",
    "en": "There is mathematics behind the bluff."
  },
  "creator.1.body": {
    "zh": "这个 Game Jam 项目以大话骰为原型。我先用概率模型处理叫牌与质疑：看似依赖吹牛和表演的游戏，同样有理性的决策基础。测试时，这套策略一度让我连续多局难以取胜；但反复交手后，我也开始摸清它的习惯。",
    "en": "This Game Jam project began with Liar’s Dice. I first used a probability model for bidding and challenging: a game of bluffing and performance also has a rational decision structure. During testing, that strategy beat me several rounds in a row. With repeated play, though, I began to recognise its habits."
  },
  "creator.2.step": {
    "zh": "02 / 设计分工",
    "en": "02 / DESIGN THE DIVISION OF LABOUR"
  },
  "creator.2.title": {
    "zh": "把性格，变成影响决策的参数。",
    "en": "Make personality a decision parameter."
  },
  "creator.2.body": {
    "zh": "在混合原型中，我让大语言模型结合手牌与当前对局记录，输出风险偏好，再由数学策略调整质疑阈值、叫牌幅度与诈唬倾向。激进或冷静，因此成为策略中的实际差异；对白与微表情则把这些差异带到玩家面前。",
    "en": "In the hybrid prototype, the language model reads the hand and the current round’s history, then outputs a risk preference. The mathematical strategy uses it to adjust challenge thresholds, bid jumps and bluffing tendencies. Boldness and caution become real differences in play; dialogue and written tells make those differences visible to the player."
  },
  "creator.3.step": {
    "zh": "03 / 验证与迭代",
    "en": "03 / TEST AND ITERATE"
  },
  "creator.3.title": {
    "zh": "让技术判断，落成可以体验的设计。",
    "en": "Turn technical judgment into an experience."
  },
  "creator.3.body": {
    "zh": "我通过实际对局检验对手能否回应玩家的行为、表现与策略是否一致，并让模型输出可以被观察和调试。当前演示继续探索大模型决策与规则校验；更长期的玩家习惯建模，是我希望推进的方向。",
    "en": "I use actual matches to test whether an opponent responds to player behaviour, whether its performance fits its strategy, and whether its outputs can be inspected and debugged. The current demo explores language-model decisions with rule validation. Longer-term modelling of player habits is a direction I want to develop further."
  },
  "creator.diagram": {
    "zh": "混合原型 / 参数驱动的决策分工",
    "en": "HYBRID PROTOTYPE / PARAMETER-DRIVEN DECISIONS"
  },
  "creator.diagramAlt": {
    "zh": "混合原型：对局信息进入语言模型，风险参数交给数学策略执行",
    "en": "Hybrid prototype: the language model reads the situation, then a mathematical strategy acts on its risk parameter"
  },
  "creator.flow.1.title": {
    "zh": "读取对局",
    "en": "Read the round"
  },
  "creator.flow.1.body": {
    "zh": "手牌、叫牌与当前对局记录",
    "en": "Hand, bids and the current round’s history"
  },
  "creator.flow.2.title": {
    "zh": "调整风险偏好",
    "en": "Set the risk preference"
  },
  "creator.flow.2.body": {
    "zh": "大语言模型输出风险参数",
    "en": "The language model outputs a risk parameter"
  },
  "creator.flow.3.title": {
    "zh": "执行数学策略",
    "en": "Run the strategy"
  },
  "creator.flow.3.body": {
    "zh": "调整阈值与评分，<br/>执行叫牌或质疑。",
    "en": "Adjust scores and thresholds.<br>Bid or challenge."
  },
  "creator.version": {
    "zh": "迭代说明：上述参数调节对应混合原型。上方可玩版本采用大模型生成行动、游戏规则校验，并利用当前对局历史；长期玩家学习尚属探索方向。",
    "en": "Iteration note: parameter tuning describes the hybrid prototype. The playable version above lets the language model propose actions, validates them against game rules, and uses history from the current round. Long-term player learning remains a direction for future work."
  },
  "creator.method": {
    "zh": "[ 我的工作方式 ]",
    "en": "[ HOW I WORK ]"
  },
  "creator.takeaway": {
    "zh": "理解原理，判断边界，设计分工。<br/>在实践中，把 AI 转化为设计价值。",
    "en": "Understand the principles. Define the boundaries. Design the roles.<br>Test them in practice—and turn AI into design value."
  },
  "closing.title": {
    "zh": "下一次，<br/>你相信哪一张脸？",
    "en": "Which mask<br>will you trust next?"
  },
  "closing.play": {
    "zh": "坐进牌桌",
    "en": "Take your seat"
  },
  "footer.brand": {
    "zh": "假面骰子 <span>/ MASKED LIAR’S DICE</span>",
    "en": "Masked Dice <span>/ MASKED LIAR’S DICE</span>"
  },
  "footer.tagline": {
    "zh": "一场关于概率、表情与信任的实验。",
    "en": "An experiment in probability, performance and trust."
  },
  "footer.top": {
    "zh": "回到顶部",
    "en": "Back to top"
  },
  "player.play": {
    "zh": "开始游戏",
    "en": "Play game"
  },
  "player.loading": {
    "zh": "正在打开牌桌…",
    "en": "Opening the table…"
  },
  "player.firstLoad": {
    "zh": "首次需下载游戏资源，请稍候。",
    "en": "Downloading the game for the first visit. Please wait."
  },
  "player.hint": {
    "zh": "就在封面里游玩 · 建议使用键盘与鼠标",
    "en": "Play right in the cover · Keyboard + mouse recommended"
  },
  "player.caption": {
    "zh": "原生 1bit 渲染 · 游戏界面为英文",
    "en": "NATIVE 1BIT RENDERING · GAME UI IN ENGLISH"
  },
  "player.ready": {
    "zh": "牌桌已就位 · 游戏界面为英文",
    "en": "THE TABLE IS READY · GAME UI IN ENGLISH"
  },
  "player.frame": {
    "zh": "假面骰子 — 在线游玩",
    "en": "Masked Dice — Play"
  },
  "player.fullscreen": {
    "zh": "全屏游玩 ↗",
    "en": "Full screen ↗"
  },
  "player.exitFullscreen": {
    "zh": "退出全屏 ↙",
    "en": "Exit full screen ↙"
  },
  "player.error": {
    "zh": "游戏未能打开，请重试或在独立窗口游玩。",
    "en": "The game could not open. Retry or open it in a new tab."
  },
  "player.retry": {
    "zh": "重新加载游戏",
    "en": "Retry game"
  },
  "language.changed": {
    "zh": "已切换为中文。",
    "en": "English selected."
  },
  "demo.readable": {
    "zh": "全场至少有 {quantity} 个 {face} 点",
    "en": "At least {quantity} dice showing {face} across the table"
  },
  "demo.die": {
    "zh": "{face} 点骰子",
    "en": "Die showing {face}"
  },
  "demo.challengeTrue": {
    "zh": "这次，你质疑错了。",
    "en": "This time, your challenge was wrong."
  },
  "demo.challengeFalse": {
    "zh": "你揭穿了这次虚张声势。",
    "en": "You caught the bluff."
  },
  "demo.trustTrue": {
    "zh": "这次，你的信任有依据。",
    "en": "This time, your trust was justified."
  },
  "demo.trustFalse": {
    "zh": "这次，他骗过了你。",
    "en": "This time, the bluff worked."
  },
  "demo.expression.0": {
    "zh": "「他垂下目光，额角的血管轻轻跳动。」",
    "en": "“He lowers his gaze. A vein at his temple pulses faintly.”"
  },
  "demo.explanation.0": {
    "zh": "紧张未必意味着撒谎。全场有 3 颗普通 6 点，加上叫牌者的特殊 6 点计作 3 颗，共 6 颗，足以支撑叫牌。",
    "en": "Nerves do not always mean a lie. Three ordinary sixes plus the bidder’s matching special die, worth three, make six—enough to support the bid."
  },
  "demo.expression.1": {
    "zh": "「他靠回椅背，嘴角扬起，视线始终没有离开你。」",
    "en": "“He leans back with a faint smile, never taking his eyes off you.”"
  },
  "demo.explanation.1": {
    "zh": "这一次，自信是在虚张声势。全场只有 3 颗普通 4 点，叫牌者的特殊骰子点数不匹配，不计入；有效数量不足 6 颗。",
    "en": "This time, confidence hides a bluff. There are only three ordinary fours. The bidder’s special die does not match and adds nothing, so the count falls short of six."
  },
  "demo.expression.2": {
    "zh": "「他的手指敲了一下骰盅，随后停住，像是在等你开口。」",
    "en": "“He taps the cup once, then holds still, as if waiting for you to speak.”"
  },
  "demo.explanation.2": {
    "zh": "他在等你质疑。4 颗普通 5 点，加上叫牌者的特殊 5 点计作 3 颗，刚好达到 7 颗。看似冒险的叫牌，可能已经算准。",
    "en": "He was waiting for a challenge. Four ordinary fives plus the bidder’s matching special die, worth three, reach exactly seven. A bid that looks reckless may be carefully calculated."
  }
};
let language='zh';
try {
  const requested=new URL(location.href).searchParams.get('lang');
  const saved=localStorage.getItem('masked-dice-language');
  language=requested==='en'||requested==='zh'?requested:saved==='en'?'en':'zh';
} catch {}
function t(key,values={}) {
  const text=messages[key]?.[language]??messages[key]?.zh??key;
  return text.replace(/\{(\w+)\}/g,(_,name)=>String(values[name]??'{'+name+'}'));
}
function apply() {
  document.documentElement.lang=language==='en'?'en':'zh-CN';
  for(const node of document.querySelectorAll('[data-i18n]'))node.textContent=t(node.dataset.i18n);
  // HTML translations are authored with this site; no visitor content is inserted.
  for(const node of document.querySelectorAll('[data-i18n-html]'))node.innerHTML=t(node.dataset.i18nHtml);
  for(const attribute of ['aria-label','alt','title','content'])for(const node of document.querySelectorAll('[data-i18n-'+attribute+']'))node.setAttribute(attribute,t(node.getAttribute('data-i18n-'+attribute)));
  for(const button of document.querySelectorAll('[data-set-lang]'))button.setAttribute('aria-pressed',String(button.dataset.setLang===language));
}
function setLanguage(next) {
  if(next!=='en'&&next!=='zh')return;
  language=next;apply();
  try {localStorage.setItem('masked-dice-language',language);}catch{}
  try {const url=new URL(location.href);if(language==='en')url.searchParams.set('lang','en');else url.searchParams.delete('lang');history.replaceState(null,'',url);}catch{}
  document.dispatchEvent(new CustomEvent('site:languagechange',{detail:{language}}));
  const status=document.getElementById('language-status');if(status)status.textContent=t('language.changed');
}
window.SiteI18n=Object.freeze({t,setLanguage,get language(){return language;}});
for(const button of document.querySelectorAll('[data-set-lang]'))button.addEventListener('click',()=>setLanguage(button.dataset.setLang));
apply();

})();
