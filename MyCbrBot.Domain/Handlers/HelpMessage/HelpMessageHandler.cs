using Insight.TelegramBot;
using Insight.TelegramBot.Handling.Handlers;
using Insight.TelegramBot.Models;
using Telegram.Bot.Types;

namespace MyCbrBot.Domain.Handlers.HelpMessage;

public sealed class HelpMessageHandler : IMatchingUpdateHandler<HelpMessageMatcher>
{
    private readonly IBot _bot;

    public HelpMessageHandler(IBot bot)
    {
        _bot = bot;
    }

    public Task Handle(Update update, CancellationToken cancellationToken = default)
    {
        return _bot.SendMessageAsync(new TextMessage(update.Message!.Chat)
        {
            Text =
                @"Я могу прислать курсы валют согласно ставке Центрального Банка России на сегодня или указанную дату, а также конвертировать валюты.

Для того, чтобы получить доступные курсы валют на сегодня по Московскому времени отправьте команду '/today'. Если вас интересует курс конкретной валюты, добавьте ее код или ISO код через пробел.
Например, '/today usd' или '/today 840' вернут курс доллара на сегодняшний день.

Для того, чтобы получить доступные курсы валют на дату по Московскому времени отправьте команду 'date' c датой через пробел в формате dd.MM.yyyy или dd-MM-yyyy. Аналогично с '/today' вы можете запросить курс по конкретной валюте, для этого добавьте ее код или ISO код через пробел после даты.
Например, '/date 01.01.2020' вернет все доступные курсы валют на 01.01.2020 соответственно. '/date 01.01.2020 840' или '/date 01.01.2020 usd' вернут курс доллара на 01.01.2020.

Для того, чтобы конвертировать валюту введите команду '/convert сумма из_валюты в_валюту дата'. Дата необязательна для ввода.
Для перевода в рубли можно отправить команду '/convert сумма из_валюты', также можно отправить команды '/convert сумма из_валюты rub' или '/convert сумма из_ваалюты 840'.
Например, '/convert 100 usd eur' произведет конвертацию из долларов в евро на дату по Московскому времени. '/convert 100 usd eur 01.01.2020' произведет конвертацию из долларов в евро на 01.01.2020'.
"
        }, cancellationToken);
    }
}