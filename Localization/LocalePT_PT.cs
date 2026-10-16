// <copyright file="LocalePT_PT.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocalePT_PT.cs
// Purpose: European Portuguese pt-PT locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// European Portuguese localization source.
    /// </summary>
    public sealed class LocalePT_PT : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        public LocalePT_PT(BBoardSettings setting)
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

            const string ToggleName = "Libertar passageiros atrasados";

            string SpeedDescription(string transitName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valores mais altos aceleram o embarque e desembarque em {transitName}.\n" +
                    "Isto reduz o tempo normal de paragem enquanto os passageiros entram e saem.\n" +
                    "<==========================>\n" +
                    "Valor de carga:\n" +
                    "1x = 100% da paragem vanilla\n" +
                    "2x = ~1/2 da paragem prevista\n" +
                    "3x = ~1/3 da paragem prevista (recomendado)\n" +
                    "5x = ~1/5 da paragem prevista (máx.)";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Estado atual: {transitName}>\n" +
                    "**À espera** = passageiros à espera agora.\n" +
                    "**Média** = tempo médio de espera.\n" +
                    "**Pior** = maior média de espera numa paragem.\n" +
                    "Se estiver alta, verifica trânsito, paragem bloqueada/com erro ou poucos veículos.\n" +
                    $"**Libertados hoje** = atribuições atrasadas libertadas por <{ToggleName}>.\n" +
                    "<Stats no Log> mostra paragens, IDs de entidade e mais detalhes.";
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
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.StatusGroup), "Estado" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutInfoGroup), "Info do mod" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Ligações" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Embarque de autocarro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "paragens de autocarro",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Embarque ferroviário" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "paragens de comboio, elétrico e metro",
                        "Aplica-se a comboios, elétricos e metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Embarque de avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminais de avião",
                        "Aplica-se a terminais de passageiros dos aviões.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Ajuda autocarros, elétricos, comboios e metro a partir quando um passageiro atribuído está claramente demasiado longe para embarcar.\n" +
                    "- Quem está na paragem/estação ou perto mantém a atribuição e continua a embarcar.\n" +
                    "- Passageiros em plataformas longas de comboio e metro ficam protegidos.\n" +
                    "- Quem está claramente longe pode ser libertado após a hora de partida e uma pequena tolerância.\n" +
                    "- Grupos/famílias têm proteção extra para crianças e animais terminarem o embarque.\n" +
                    "- Navios, ferries e aviões mantêm o tratamento vanilla para passageiros atrasados.\n" +
                    "Os cims libertados ficam na cidade e continuam ou recalculam a viagem normalmente."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims correm mais cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cims já atribuídos começam a <correr mais cedo> para apanhar autocarro, elétrico, comboio ou metro antes da partida.\n" +
                    "- Ajuda em plataformas longas e na aproximação à paragem.\n" +
                    "- Começa até cerca de 3 minutos de jogo antes do vanilla.\n" +
                    "- Só afeta cims já atribuídos a um veículo em embarque.\n" +
                    "- Não teletransporta, força embarque nem altera a hora de partida."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Utilização total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Utilização mensal dos transportes públicos na vista de Transportes do jogo.\n" +
                    "A hora de atualização indica quando este estado foi captado (normalmente ao abrir Opções)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims + cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Se ativo [x], conta os cims de hoje que Better Boarding fez **correr mais cedo** para apanhar autocarro, elétrico, comboio ou metro.\n" +
                    "Run Sooner começa até cerca de 3 minutos de jogo antes do vanilla."
                },

                // Status rows
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusBus)), "Autocarro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusBus)), StatusDescription("autocarro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTram)), "Elétrico" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTram)), StatusDescription("elétrico") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusTrain)), "Comboio" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusTrain)), StatusDescription("comboio") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusSubway)), "Metro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusSubway)), StatusDescription("metro") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusFerry)), "Ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusFerry)), StatusDescription("ferry") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusShip)), "Navio" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusShip)), StatusDescription("navio") },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusAir)), "Avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusAir)), StatusDescription("avião") },

                // Status buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats no Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Escreve um relatório detalhado único em **BetterBoarding.log**.\n" +
                    "Inclui totais de espera, 3 piores paragens por modo, exemplos de passageiros libertados, IDs de entidade e pistas de linha."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Abrir Log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Abre **BetterBoarding.log** se existir.\n" +
                    "Se ainda não existir, abre a pasta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nome apresentado do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão atual do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre a página do autor no Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Log detalhado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Apenas debug / testes**\n" +
                    "Normalmente mantém **DESLIGADO**.\n" +
                    "Adiciona detalhes <live> a <Logs/BetterBoarding.log> enquanto a cidade corre.\n" +
                    "**Não usar no jogo normal.**\n" +
                    "Pode reduzir o desempenho e criar logs enormes.\n" +
                    "Os logs antigos podem ser apagados depois.\n" +
                    "<Stats no Log> escreve só um instantâneo e tem pouco impacto comparado com o log detalhado.\n" +
                    "Ativa 10-15 min se precisares de uma cronologia; gera muitos dados.\n" +
                    "Desliga novamente antes de jogar normalmente."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Estado não carregado." },
                { WaitStatus.KeyNoCityLoaded, "Nenhuma cidade carregada." },
                { WaitStatus.KeyNoStopsFound, "Nenhuma paragem encontrada." },

                { WaitStatus.KeyStatusLine, "{0} espera | méd. {1} | pior {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} libert. hoje" },
                { WaitStatus.KeyStatusSkipOff, "libertar OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} tur./mês | {1} cid./mês | atual. {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr cedo OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Relatório pedido, mas nenhuma cidade está carregada." },
                { WaitStatus.KeyReportTitle, "Instantâneo Stats no Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Definições: {0}" },
                { WaitStatus.KeyReportNote, "A pista da linha vem do waypoint com maior espera nessa paragem." },
                { WaitStatus.KeyReportTesterHintsHeader, "Dicas de teste" },
                { WaitStatus.KeyReportHintWorstStops, "Piores paragens: verifica primeiro no jogo ou com Scene Explorer (localiza pelo ID da entidade). Vê trânsito, localização má ou paragem com erro." },
                { WaitStatus.KeyReportHintSkippedCims, "Atribuições solo libertadas: cada contagem é uma atribuição de veículo libertada após verificar a distância. Usa os exemplos de seguimento para ver o que aconteceu depois." },
                { WaitStatus.KeyReportHintLateGroups, "Embarque de grupos: mostra famílias/grupos ainda incompletos no momento do relatório. Better Boarding dá tolerância extra e ajuda membros atrasados a terminar o embarque." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paragens servidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paragens com passageiros à espera: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passageiros à espera: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera média: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Atribuições solo libertadas hoje: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Pior paragem: nenhuma; ninguém está à espera agora." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera média da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nome da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidade da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidade waypoint da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Pista da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidade da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Média do waypoint da pior linha: {0} com {1} à espera" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} piores paragens por espera média:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | méd. {2} | espera {3} | paragem {4} | waypoint {5} | linha {6} | pista {7}" },
                { WaitStatus.KeyReportLateGroups, "Embarque de grupos incompleto: {0} passageiros em {1} grupos e {2} veículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemplos de passageiros solo libertados" },
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
