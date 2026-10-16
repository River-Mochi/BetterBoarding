// <copyright file="LocalePT_BR.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocalePT_BR.cs
// Purpose: Brazilian Portuguese pt-BR locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Brazilian Portuguese localization source.
    /// </summary>
    public sealed class LocalePT_BR : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocalePT_BR(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName + " (Embarque melhorado)";

            const string ToggleName = "Não esperar passageiros atrasados";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valores maiores aceleram embarque e desembarque em {transitName}.\n" +
                    "Isso reduz o tempo normal de parada enquanto passageiros entram e saem.\n" +
                    "<==========================>\n" +
                    "1x = 100% da parada vanilla\n" +
                    "2x = ~1/2 da parada planejada\n" +
                    "3x = ~1/3 da parada planejada (recomendado)\n" +
                    "5x = ~1/5 da parada planejada (máx.)";
            }

            // One helper keeps the status tooltips consistent while explaining
            // which modes Better Boarding can actually skip late passengers for.
            string StatusDescription(string transitName, bool skipSupported)
            {
                string skippedText = skipSupported
                    ? "**Ignorados** = passageiros atrasados ignorados hoje para o transporte não ficar esperando.\n" +
                      "Better Boarding age bem antes dos ~10 minutos que o vanilla leva para liberar um veículo travado.\n" +
                      $"Controlado por <{ToggleName}>.\n"
                    : "**Ignorados** = 0 neste modo. Aqui, passageiros atrasados ficam com o vanilla.\n";

                return
                    $"<Status atual: {transitName}>\n" +
                    "**Esperando** = passageiros esperando agora.\n" +
                    "**Média** = tempo médio de espera.\n" +
                    "**Pior** = maior média de espera em uma parada.\n" +
                    "Se estiver alta, confira trânsito, parada bloqueada/com bug ou poucos veículos.\n" +
                    skippedText +
                    "<Stats no Log> mostra paradas, IDs de entidade e mais detalhes.";
            }

            return new Dictionary<string, string>
            {
                // Options mod name
                { m_Setting.GetSettingsLocaleID(), title },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.ActionsTab), "Ações" },
                { m_Setting.GetOptionTabLocaleID(BBoardSettings.AboutTab), "Sobre" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.SpeedGroup), "Velocidade de embarque" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.BehaviorGroup), "Comportamento" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Status" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info do mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Links" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Embarque de ônibus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "paradas de ônibus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Embarque ferroviário" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "paradas de trem, bonde e metrô",
                        "Vale para trem, bonde e metrô.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Embarque de avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminais de avião",
                        "Vale para terminais de aviões de passageiros.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Um passageiro atrasado longe demais da parada pode ser ignorado para o transporte não ficar esperando.\n" +
                    "Better Boarding age bem antes dos ~10 minutos que o vanilla leva para liberar um veículo travado.\n" +
                    "- Passageiros na parada/estação ou perto continuam atribuídos e embarcando.\n" +
                    "- Plataformas longas de trem/metrô continuam protegidas.\n" +
                    "- Grupos/famílias têm proteção extra.\n" +
                    "- Cims ignorados continuam na cidade; o vanilla pode continuar ou recalcular a viagem, muitas vezes em um veículo posterior.\n" +
                    "- Navios, balsas e aviões mantêm o tratamento vanilla para passageiros atrasados."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims correm mais cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cims atribuídos começam a <correr mais cedo> para pegar o transporte antes de ele sair.\n" +
                    "- Começa até cerca de 3 minutos no jogo antes do vanilla.\n" +
                    "- Ajuda em plataformas longas e ao se aproximar da parada.\n" +
                    "- Só afeta cims já atribuídos a um veículo embarcando passageiros.\n" +
                    "- Não teleporta cims nem muda horários de saída."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensal do transporte público pela visão de Transporte do jogo.\n" +
                    "Atualizado = momento em que este status foi capturado."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims + cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Conta os cims que Better Boarding fez **correr mais cedo** hoje.\n" +
                    "Até cerca de 3 minutos no jogo antes do vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Ônibus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("ônibus", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Bonde" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("bonde", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Trem" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("trem", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metrô" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metrô", skipSupported: true) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Balsa" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("balsa", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Navio" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("navio", skipSupported: false) },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avião", skipSupported: false) },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats no Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Grava um relatório detalhado único em **BetterBoarding.log**.\n" +
                    "Inclui totais de espera, as 3 piores paradas, exemplos de passageiros atrasados ignorados, Entity IDs e dicas de linha."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Abrir Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Abre **BetterBoarding.log** se existir.\n" +
                    "Se ainda não existir, abre a pasta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nome exibido do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão atual do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre a página do autor no Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Log detalhado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**SOMENTE Debug / Teste**\n" +
                    "Deixe DESLIGADO no jogo normal.\n" +
                    "Adiciona detalhes <ao vivo> em <Logs/BetterBoarding.log> enquanto a cidade roda.\n" +
                    "Pode reduzir o desempenho e criar arquivos de log grandes.\n" +
                    "Use por 10-15 min para diagnosticar e depois DESLIGUE de novo.\n" +
                    "<Stats no Log> é um snapshot único mais leve."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status não carregado." },
                { WaitStatus.KeyNoCityLoaded, "Nenhuma cidade carregada." },
                { WaitStatus.KeyNoStopsFound, "Nenhuma parada encontrada." },

                { WaitStatus.KeyStatusLine, "{0} espera | méd. {1} | pior {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} atrasados ignorados" },
                { WaitStatus.KeyStatusSkipOff, "liberar OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tur./mês | {1} cid./mês | {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr cedo OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Relatório solicitado, mas nenhuma cidade está carregada." },
                { WaitStatus.KeyReportTitle, "Instantâneo Stats no Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Configurações: {0}" },
                { WaitStatus.KeyReportNote, "A dica de linha vem do waypoint com maior espera nessa parada." },
                { WaitStatus.KeyReportTesterHintsHeader, "Dicas de teste" },
                { WaitStatus.KeyReportHintWorstStops, "Piores paradas: verifique primeiro no jogo ou com Scene Explorer (localize pelo ID da entidade). Confira trânsito, localização ruim ou parada com bug." },
                { WaitStatus.KeyReportHintSkippedCims, "Passageiros atrasados ignorados: estavam longe demais após a saída, então Better Boarding impediu o veículo de continuar esperando por eles. Os exemplos mostram o que aconteceu depois." },
                { WaitStatus.KeyReportHintLateGroups, "Embarque de grupos: mostra famílias/grupos ainda incompletos quando o relatório é feito. Better Boarding dá tolerância extra e ajuda membros atrasados a terminar o embarque." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paradas atendidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paradas com passageiros esperando: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passageiros esperando: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera média: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Passageiros atrasados ignorados hoje: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Pior parada: nenhuma; ninguém está esperando agora." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera média da pior parada: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nome da pior parada: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidade da pior parada: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidade waypoint da pior parada: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Dica da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidade da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Média do waypoint da pior linha: {0} com {1} esperando" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} piores paradas por espera média:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | méd. {2} | esperando {3} | parada {4} | waypoint {5} | linha {6} | dica {7}" },
                { WaitStatus.KeyReportLateGroups, "Embarque de grupos incompleto: {0} passageiros em {1} grupos e {2} veículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemplos de passageiros atrasados ignorados" },
                { WaitStatus.KeyReportLastSkippedSampleLine, "{0}. {1} | passageiro {2} | veículo perdido {3} | hora {4} | agora {5}" },
                { WaitStatus.KeyReportNone, "nenhum" },
                { WaitStatus.KeyReportUnknown, "(desconhecido)" },
            };
        }

        public void Unload()
        {
        }
    }
}
