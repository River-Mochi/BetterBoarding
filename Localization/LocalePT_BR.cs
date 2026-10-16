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
            string title = Mod.ModName;

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Liberar passageiros atrasados";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valores maiores aceleram embarque e desembarque em {transitName}.\n" +
                    "Isso reduz o tempo normal de parada enquanto passageiros entram e saem.\n" +
                    "<==========================>\n" +
                    "Valor de carga:\n" +
                    "1x = 100% da parada vanilla\n" +
                    "2x = ~1/2 da parada planejada\n" +
                    "3x = ~1/3 da parada planejada (recomendado)\n" +
                    "5x = ~1/5 da parada planejada (máx.)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Status atual: {transitName}>\n" +
                    "**Esperando** = passageiros esperando agora.\n" +
                    "**Média** = tempo médio de espera.\n" +
                    "**Pior** = maior média de espera em uma parada.\n" +
                    "Se estiver alta, confira trânsito, parada bloqueada/com bug ou poucos veículos.\n" +
                    $"**Liberados hoje** = atribuições atrasadas liberadas por <{ToggleName}>.\n" +
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
                    "Ajuda ônibus, bondes, trens e metrôs a partir quando um passageiro atribuído está claramente longe demais para embarcar.\n" +
                    "- Quem está na parada/estação ou por perto continua atribuído e embarcando.\n" +
                    "- Passageiros em plataformas longas de trem e metrô ficam protegidos.\n" +
                    "- Quem está claramente longe pode ser liberado após o horário de partida e uma pequena tolerância.\n" +
                    "- Grupos/famílias recebem proteção extra para crianças e pets terminarem o embarque.\n" +
                    "- Navios, balsas e aviões mantêm o tratamento vanilla para passageiros atrasados.\n" +
                    "Cims liberados continuam na cidade e seguem ou recalculam a viagem normalmente."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims correm mais cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cims já atribuídos começam a <correr mais cedo> para pegar ônibus, bonde, trem ou metrô antes da partida.\n" +
                    "- Ajuda em plataformas longas e na aproximação da parada.\n" +
                    "- Começa até cerca de 3 minutos do jogo antes do vanilla.\n" +
                    "- Só afeta cims já atribuídos a um veículo em embarque.\n" +
                    "- Não teleporta, força embarque nem muda o horário de partida."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensal do transporte público pela visão de Transporte do jogo.\n" +
                    "O horário atualizado mostra quando este status foi capturado (geralmente ao abrir Opções)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims + cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Se ativo [x], conta os cims de hoje que Better Boarding fez **correr mais cedo** para pegar ônibus, bonde, trem ou metrô.\n" +
                    "Run Sooner começa até cerca de 3 minutos do jogo antes do vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Ônibus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("ônibus") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Bonde" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("bonde") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Trem" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("trem") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metrô" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metrô") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Balsa" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("balsa") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Navio" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("navio") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avião") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats no Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Grava um relatório detalhado único em **BetterBoarding.log**.\n" +
                    "Inclui totais de espera, 3 piores paradas por modo, exemplos de passageiros liberados, IDs de entidade e dicas de linha."
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
                    "**Somente debug / testes**\n" +
                    "Normalmente deixe **DESLIGADO**.\n" +
                    "Adiciona detalhes <live> a <Logs/BetterBoarding.log> enquanto a cidade roda.\n" +
                    "**Não use no jogo normal.**\n" +
                    "Pode reduzir desempenho e criar logs enormes.\n" +
                    "Logs antigos podem ser apagados depois.\n" +
                    "<Stats no Log> grava só um instantâneo e tem pouco impacto comparado ao log detalhado.\n" +
                    "Ative por 10-15 min se precisar de uma linha do tempo; gera muitos dados.\n" +
                    "Desligue novamente antes de jogar normalmente."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status não carregado." },
                { WaitStatus.KeyNoCityLoaded, "Nenhuma cidade carregada." },
                { WaitStatus.KeyNoStopsFound, "Nenhuma parada encontrada." },

                { WaitStatus.KeyStatusLine, "{0} espera | méd. {1} | pior {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} liberados hoje" },
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
                { WaitStatus.KeyReportHintSkippedCims, "Atribuições solo liberadas: cada contagem é uma atribuição de veículo liberada após a verificação de distância. Use os exemplos de acompanhamento para ver o que aconteceu depois." },
                { WaitStatus.KeyReportHintLateGroups, "Embarque de grupos: mostra famílias/grupos ainda incompletos quando o relatório é feito. Better Boarding dá tolerância extra e ajuda membros atrasados a terminar o embarque." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paradas atendidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paradas com passageiros esperando: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passageiros esperando: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera média: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Atribuições solo liberadas hoje: {0}" },
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
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemplos de passageiros solo liberados" },
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
