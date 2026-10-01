-- MS_Description is stored in SQL Server metadata. This script can be rerun
-- after editing the text: existing descriptions are updated.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @descriptions TABLE
(
    table_name  SYSNAME NOT NULL,
    column_name SYSNAME NULL,
    description NVARCHAR(4000) NOT NULL
);

INSERT INTO @descriptions (table_name, column_name, description)
VALUES
    (N'currencies', NULL, N'Справочник валют платежей и комиссий. Одна запись соответствует одному коду валюты.'),
    (N'countries', NULL, N'Справочник стран, используемых в платежах, ограничениях провайдеров и правилах комиссий.'),
    (N'webhook_endpoints', NULL, N'Настроенные адреса получателей исходящих вебхуков системы.'),
    (N'providers', NULL, N'Справочник платежных провайдеров. Один провайдер может иметь несколько подключенных аккаунтов.'),
    (N'provider_accounts', NULL, N'Конкретное подключение к платежному провайдеру, через которое выполняются операции.'),
    (N'provider_credentials', NULL, N'Зашифрованные учетные данные аккаунта провайдера. Тип учетных данных уникален в пределах аккаунта.'),
    (N'provider_capabilities', NULL, N'Возможности провайдера. Одна запись связывает провайдера с одной возможностью.'),
    (N'provider_supported_countries', NULL, N'Страны, поддерживаемые провайдером. Одна запись соответствует одной паре провайдер–страна.'),
    (N'provider_supported_currencies', NULL, N'Валюты, поддерживаемые провайдером. Одна запись соответствует одной паре провайдер–валюта.'),
    (N'provider_fee_rules', NULL, N'Правила комиссий конкретного аккаунта провайдера с возможными условиями по стране, валюте, сумме и сроку действия.'),
    (N'provider_health_checks', NULL, N'Результаты отдельных проверок доступности аккаунта провайдера во времени.'),
    (N'routing_policies', NULL, N'Именованные политики маршрутизации платежей. Правила хранятся в версиях политики.'),
    (N'routing_policy_versions', NULL, N'Версии политики маршрутизации с датами активации и деактивации.'),
    (N'routing_rules', NULL, N'Правила в конкретной версии политики маршрутизации. Условия и действия хранятся отдельно.'),
    (N'routing_rule_conditions', NULL, N'Отдельные условия правила маршрутизации: поле, оператор и значение для сравнения.'),
    (N'routing_rule_actions', NULL, N'Аккаунты провайдеров, предусмотренные действием правила маршрутизации, с приоритетом и возможным весом.'),
    (N'payments', NULL, N'Платежи, принятые системой для обработки. Здесь хранится текущее состояние; отдельные обращения к провайдерам хранятся в payment_attempts.'),
    (N'payment_method_tokens', NULL, N'Токены платежных методов, выданные провайдером и привязанные к его аккаунту.'),
    (N'payment_attempts', NULL, N'Отдельные попытки провести платеж через конкретный аккаунт провайдера. У платежа может быть несколько попыток.'),
    (N'payment_operations', NULL, N'Операции над платежом, например авторизация или списание. Операция может ссылаться на конкретную попытку.'),
    (N'payment_status_history', NULL, N'История изменений статуса платежа. Одна запись фиксирует один переход или установку статуса.'),
    (N'three_ds_sessions', NULL, N'Сеансы 3-D Secure, связанные с попыткой платежа и взаимодействием с провайдером.'),
    (N'routing_decisions', NULL, N'Результаты применения версии политики маршрутизации к платежу, включая выбранный аккаунт провайдера, если выбор сделан.'),
    (N'routing_decision_candidates', NULL, N'Аккаунты провайдеров, рассмотренные в одном решении маршрутизации, с оценкой пригодности и причиной исключения.'),
    (N'refunds', NULL, N'Запросы на возврат средств по платежу и конкретной попытке его обработки.'),
    (N'refund_attempts', NULL, N'Отдельные попытки выполнить возврат через аккаунт провайдера исходной попытки платежа.'),
    (N'idempotency_keys', NULL, N'Ключи идемпотентности запросов для распознавания повторных обращений и связи с созданным ресурсом.'),
    (N'deduplication_records', NULL, N'Факты обработки внешних событий, уникальные по источнику и внешнему идентификатору события.'),
    (N'webhook_events', NULL, N'Входящие вебхуки от платежных провайдеров с исходным содержимым и состоянием обработки.'),
    (N'outbound_webhook_events', NULL, N'Исходящие события системы для отправки вебхуков. Попытки доставки каждому адресу хранятся отдельно.'),
    (N'webhook_delivery_attempts', NULL, N'Отдельные попытки доставить исходящее событие конкретному адресу вебхука.'),
    (N'outbox_messages', NULL, N'Сообщения, ожидающие публикации из локальной базы данных во внешнюю систему обмена сообщениями.'),
    (N'inbox_messages', NULL, N'Факты обработки сообщений конкретными потребителями; составной ключ защищает от повторной обработки.'),
    (N'reconciliation_jobs', NULL, N'Запуски сверки операций с конкретным аккаунтом провайдера и итоговые счетчики проверки.'),
    (N'reconciliation_items', NULL, N'Результаты сверки отдельных попыток платежей: локальный статус, статус провайдера и решение по расхождению.'),
    (N'dead_letter_messages', NULL, N'Сообщения, обработка которых завершилась неудачей и которые требуют отдельного разбора или повторной обработки.'),
    (N'audit_log', NULL, N'Журнал изменений сущностей: исполнитель, действие, время и при наличии состояние до и после изменения.'),

    (N'currencies', N'decimal_places', N'Число десятичных знаков валюты в справочнике. Ограничение точности денежных сумм по этому полю пока не реализовано.'),
    (N'webhook_endpoints', N'secret', N'Секрет для взаимодействия с получателем исходящих вебхуков; способ применения определяется приложением.'),
    (N'provider_accounts', N'external_account_id', N'Идентификатор аккаунта в системе провайдера, если он предоставлен.'),
    (N'provider_accounts', N'is_sandbox', N'Признак тестового окружения аккаунта провайдера.'),
    (N'provider_credentials', N'encrypted_value', N'Зашифрованное значение учетных данных. Формат и способ шифрования определяются приложением.'),
    (N'provider_fee_rules', N'percent_fee', N'Процентная составляющая комиссии; точная трактовка единиц и порядок расчета пока не закреплены схемой.'),
    (N'provider_fee_rules', N'fixed_fee', N'Фиксированная составляющая комиссии. Валюта этого значения отдельно в схеме не задана.'),
    (N'provider_fee_rules', N'country_code', N'Необязательное ограничение правила по стране. Значение NULL допустимо; смысл NULL при выборе правила определяется приложением.'),
    (N'provider_fee_rules', N'currency_code', N'Необязательное ограничение правила по валюте. Значение NULL допустимо; смысл NULL при выборе правила определяется приложением.'),
    (N'provider_fee_rules', N'valid_to', N'Время окончания действия правила, если оно ограничено; включительность границы схемой не определена.'),
    (N'routing_policy_versions', N'activated_at', N'Время активации версии, если она активировалась. Схема не обеспечивает единственность активной версии.'),
    (N'routing_rules', N'priority', N'Числовой приоритет правила. Порядок обхода правил при равных или разных значениях задает приложение.'),
    (N'routing_rule_conditions', N'field_name', N'Имя проверяемого поля; допустимые имена пока не ограничены справочником или CHECK.'),
    (N'routing_rule_conditions', N'operator', N'Оператор сравнения; допустимые значения и их семантика пока задаются приложением.'),
    (N'routing_rule_conditions', N'value', N'Значение условия в текстовом виде; преобразование к нужному типу выполняет приложение.'),
    (N'routing_rule_actions', N'weight', N'Необязательный неотрицательный вес аккаунта; способ его применения задает алгоритм маршрутизации.'),
    (N'payments', N'external_payment_id', N'Идентификатор платежа во внешней системе. Непустые значения уникальны; NULL допускается для нескольких платежей.'),
    (N'payments', N'updated_at', N'Время последнего изменения платежа. DEFAULT задает значение при вставке; последующие обновления должно отражать приложение.'),
    (N'payment_method_tokens', N'provider_token', N'Токен платежного метода, выданный провайдером для указанного аккаунта; не является номером карты.'),
    (N'payment_attempts', N'attempt_number', N'Положительный порядковый номер попытки в пределах одного платежа.'),
    (N'payment_attempts', N'payment_method_token_id', N'Необязательный токен платежного метода; ограничение БД требует совпадения аккаунта провайдера с попыткой.'),
    (N'payment_operations', N'payment_attempt_id', N'Необязательная ссылка на попытку, которая по ограничению БД должна принадлежать этому платежу.'),
    (N'payment_status_history', N'old_status', N'Предыдущий статус платежа, если он известен; NULL допустим, например при первой записи истории.'),
    (N'routing_decisions', N'selected_provider_account_id', N'Выбранный аккаунт провайдера; если задан, должен присутствовать среди кандидатов этого решения.'),
    (N'routing_decision_candidates', N'is_eligible', N'Признак пригодности аккаунта как кандидата для данного решения маршрутизации.'),
    (N'routing_decision_candidates', N'estimated_fee', N'Оценка комиссии кандидата. Валюта и метод расчета отдельно в схеме не зафиксированы.'),
    (N'refunds', N'payment_attempt_id', N'Попытка обработки, по которой выполняется возврат; ограничение БД требует ее принадлежности указанному платежу.'),
    (N'refund_attempts', N'provider_account_id', N'Аккаунт провайдера исходной попытки платежа; совпадение обеспечивается ограничением БД.'),
    (N'idempotency_keys', N'request_hash', N'Отпечаток запроса для сопоставления повторного обращения с исходным содержимым.'),
    (N'webhook_events', N'signature_valid', N'Результат проверки подписи входящего вебхука, если проверка проводилась.'),
    (N'outbound_webhook_events', N'resource_id', N'Идентификатор ресурса, с которым связано исходящее событие, если такой ресурс указан.'),
    (N'webhook_delivery_attempts', N'attempt_number', N'Положительный номер попытки для пары исходящее событие–адрес получателя.'),
    (N'outbox_messages', N'processed_at', N'Время отметки сообщения обработанным; NULL означает, что отметка еще не установлена.'),
    (N'inbox_messages', N'consumer_name', N'Имя потребителя сообщения; одно сообщение может быть обработано несколькими разными потребителями.'),
    (N'reconciliation_items', N'resolution', N'Результат или способ разрешения обнаруженного расхождения, если решение принято.'),
    (N'dead_letter_messages', N'resolved_at', N'Время отметки проблемы как разрешенной; NULL означает отсутствие такой отметки.'),
    (N'audit_log', N'actor', N'Идентификатор или имя инициатора изменения, если он известен.'),
    (N'audit_log', N'old_value', N'Предыдущее состояние сущности в JSON, если оно сохранено.'),
    (N'audit_log', N'new_value', N'Новое состояние сущности в JSON, если оно сохранено.');

-- Fail before changing metadata if the schema is incomplete or differs from the DDL.
IF EXISTS
(
    SELECT 1
    FROM @descriptions AS d
    WHERE OBJECT_ID(N'dbo.' + QUOTENAME(d.table_name), N'U') IS NULL
       OR (d.column_name IS NOT NULL AND NOT EXISTS
           (
               SELECT 1
               FROM sys.columns AS c
               WHERE c.object_id = OBJECT_ID(N'dbo.' + QUOTENAME(d.table_name), N'U')
                 AND c.name = d.column_name
           ))
)
    THROW 50000, N'Описания не добавлены: сначала создайте все таблицы и столбцы из файлов 01–11 в текущей базе данных.', 1;

DECLARE @table_name SYSNAME;
DECLARE @column_name SYSNAME;
DECLARE @description NVARCHAR(4000);
DECLARE @object_id INT;
DECLARE @column_id INT;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE descriptions_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT table_name, column_name, description
        FROM @descriptions;

    OPEN descriptions_cursor;
    FETCH NEXT FROM descriptions_cursor INTO @table_name, @column_name, @description;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @object_id = OBJECT_ID(N'dbo.' + QUOTENAME(@table_name), N'U');
        SET @column_id = 0;

        IF @column_name IS NOT NULL
            SELECT @column_id = c.column_id
            FROM sys.columns AS c
            WHERE c.object_id = @object_id AND c.name = @column_name;

        IF EXISTS
        (
            SELECT 1
            FROM sys.extended_properties AS ep
            WHERE ep.class = 1
              AND ep.major_id = @object_id
              AND ep.minor_id = @column_id
              AND ep.name = N'MS_Description'
        )
        BEGIN
            IF @column_name IS NULL
                EXEC sys.sp_updateextendedproperty
                    @name = N'MS_Description', @value = @description,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = @table_name;
            ELSE
                EXEC sys.sp_updateextendedproperty
                    @name = N'MS_Description', @value = @description,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = @table_name,
                    @level2type = N'COLUMN', @level2name = @column_name;
        END
        ELSE
        BEGIN
            IF @column_name IS NULL
                EXEC sys.sp_addextendedproperty
                    @name = N'MS_Description', @value = @description,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = @table_name;
            ELSE
                EXEC sys.sp_addextendedproperty
                    @name = N'MS_Description', @value = @description,
                    @level0type = N'SCHEMA', @level0name = N'dbo',
                    @level1type = N'TABLE', @level1name = @table_name,
                    @level2type = N'COLUMN', @level2name = @column_name;
        END;

        FETCH NEXT FROM descriptions_cursor INTO @table_name, @column_name, @description;
    END;

    CLOSE descriptions_cursor;
    DEALLOCATE descriptions_cursor;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
