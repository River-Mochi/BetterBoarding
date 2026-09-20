// <copyright file="LocalePT_PT.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the GNU General Public License v3.0 or later,
// with the Cities: Skylines II Linking Exception.
// See LICENSE and LICENSE-EXCEPTION in the project root.
// This notice MUST be kept with copies or substantial portions of this code.
// ================= </copyright> ======================

// File: Localization/LocalePT_PT.cs
// Purpose: Portuguese (Portugal) pt-PT locale entries for Better Boarding.

namespace BetterBoarding
{
    using System.Collections.Generic;
    using Colossal;

    /// <summary>
    /// Portuguese (Portugal) localization source.
    /// </summary>
    public sealed class LocalePT_PT : IDictionarySource
    {
        private readonly BBoardSettings m_Setting;

        /// <summary>
        /// Constructs the Portuguese (Portugal) locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocalePT_PT(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Portuguese (Portugal) localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            const string ToggleName = "Ignorar passageiros atrasados";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valores mais altos reduzem o tempo de embarque e carregamento em {transitName}.\n" +
        
                    "Ajuda as filas normais a andar mais depressa, mas um passageiro atrasado ainda pode atrasar a partida devido ao funcionamento do vanilla.\n" +
                    $"Use [✓] <{ToggleName}> se quiser que cims atrasados possam perder o veículo depois da hora de partida.\n" +
                    "Os cidadãos atrasados ignorados não são apagados; o vanilla volta a encaminhá-los naturalmente.\n" +
                    "<==========================>\n" +
                    "Valor de carregamento:\n" +
                    "1x = 100% tempo de paragem vanilla\n" +
                    "2x = ~1/2 tempo de paragem planeado\n" +
                    "3x = ~1/3 tempo de paragem planeado (recomendado)\n" +
                    "5x = ~1/5 tempo de paragem planeado (máx.)\n" +
                    $"Isto não é o mesmo que <{ToggleName}>; essa opção decide se cims atrasados podem perder o {shortName} depois da hora de partida.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Estado atual de {transitName}>\n" +
                    "**À espera** = total de passageiros à espera neste momento.\n" +
                    "**Média** = tempo médio de espera desses passageiros.\n" +
                    "**Pior** paragem = maior espera média numa só paragem.\n" +
                    "As piores paragens são bons locais para verificar acidentes, trânsito bloqueado, paragens com problemas ou falta de veículos.\n" +
                    $"**Atrasados hoje** = passageiros solo atrasados ignorados hoje por <{ToggleName}>.\n" +
                    "Use <Stats to Log> para um relatório detalhado: nomes das paragens, IDs de entidades e mais.";
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
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.AboutLinksGroup), "Links" },
                { m_Setting.GetOptionGroupLocaleID(BBoardSettings.DebugGroup), "Debug" },

                // Boarding speed sliders
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Velocidade de embarque do autocarro" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "paragem de autocarro",
                        "autocarro",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Velocidade de embarque ferroviário" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "paragem de comboio, elétrico e metro",
                        "veículo",
                        "Aplica-se a paragens de comboio, elétrico e metro.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Velocidade de navio + ferry" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "paragem de navio e ferry",
                        "veículo",
                        "Aplica-se a paragens de navio e ferry.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Velocidade de avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminal de avião",
                        "avião",
                        "Aplica-se a terminais de aviões de passageiros.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "<Passageiros atrasados> que continuem <não prontos> depois da <hora de partida> podem perder o veículo.\n" +
                    "- Nota: só ignoramos cidadãos solo atrasados.\n" +
                    "- Grupos/famílias que viajam juntos e estão atrasados <não são ignorados> e podem continuar a atrasar o transporte como no vanilla.\n" +
                    "- Os grupos são uma pequena parte da multidão; a maior parte do benefício vem de ignorar cims solo atrasados.\n" +
                    "- Os cidadãos atrasados ignorados não são apagados; o jogo volta a atribuí-los naturalmente."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims correm mais cedo: Autocarro + elétrico + comboio" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cidadãos <atrasados> começam a <correr mais cedo> para tentar chegar **antes** da hora de partida.\n" +
                    "- Funciona com autocarros, elétricos e comboios, sobretudo em plataformas de comboio longas.\n" +
                    "- Só afeta cims já atribuídos a um veículo que está a embarcar.\n" +
                    "- No vanilla, os cims só começam a correr à hora de partida, o que pode ser tarde demais.\n" +
                    $"- Funciona bem com <{ToggleName}> porque pode reduzir quantos cims perdem o veículo e precisam de nova atribuição.\n" +
                    "- Não altera a hora de partida do veículo, não força o embarque nem teletransporta cidadãos."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Utilização total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Utilização mensal do transporte público da vista de Transporte do jogo.\n" +
                    "A hora de atualização mostra quando este estado foi recolhido (normalmente ao abrir as Opções)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims correm mais cedo" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Se estiver ativo [x], conta todos os cims (hoje) que começaram a **correr mais cedo** para apanhar um autocarro, elétrico ou comboio antes da partida.\n" +
                    "Os cims correm 512 frames mais cedo do que no vanilla (~2-8 segundos mais cedo em tempo real, ~2 minutos no jogo)."
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats para o log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Escreve um relatório único detalhado em **BetterBoarding.log**.\n" +
                    "Inclui totais em espera, as 3 piores paragens por modo, exemplos de cims ignorados, IDs de entidades e pistas de linha."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Abrir log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Abre **BetterBoarding.log** se existir.\n" +
                    "Se o ficheiro ainda não existir, abre a pasta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nome apresentado do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão atual do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Mochi's Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre a página do autor no Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ativar registo detalhado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Apenas debug / testes**\n" +
                    "Adiciona detalhes <ao vivo> a <Logs/BetterBoarding.log> enquanto a cidade corre.\n" +
                    "**Não ative durante jogo normal.**\n" +
                    "Deixar isto ligado pode reduzir o desempenho e criar ficheiros de log enormes.\n" +
                    "Pode apagar ficheiros de log antigos mais tarde.\n" +
                    "Nota: <Stats to Log> é um relatório pontual mais os contadores de atrasados ignorados de hoje; é diferente do registo detalhado.\n" +
                    "Use o registo detalhado durante 15-20 min se quiser uma linha temporal do que aconteceu.\n" +
                    "Não se esqueça de voltar a desligar o registo detalhado antes de jogar normalmente."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Estado não carregado." },
                { WaitStatus.KeyNoCityLoaded, "Nenhuma cidade carregada." },
                { WaitStatus.KeyNoStopsFound, "Nenhuma paragem encontrada." },

                { WaitStatus.KeyStatusLine, "{0} à espera | média {1} | pior {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} atrasados hoje" },
                { WaitStatus.KeyStatusSkipOff, "ignorar OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} turistas/mês | {1} cidadãos/mês | atualizado {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr mais cedo OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Relatório pedido, mas não há cidade carregada." },
                { WaitStatus.KeyReportTitle, "Snapshot Stats to Log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Definições: {0}" },
                { WaitStatus.KeyReportNote, "A pista de linha vem do waypoint com maior espera nessa paragem." },
                { WaitStatus.KeyReportTesterHintsHeader, "Dicas para testes" },
                { WaitStatus.KeyReportHintWorstStops, "Piores paragens: veja primeiro no jogo ou com o mod Scene Explorer (localize pelo ID da entidade). Procure trânsito, má localização da paragem ou uma paragem com problemas." },
                { WaitStatus.KeyReportHintSkippedCims, "Cims solo ignorados: passageiros atrasados que ignoramos para deixar o transporte partir. Depois, o estado normalmente deve passar para 'tem caminho' ou 'atribuído'. Se continuar 'sem caminho', inspecione a entidade mais tarde." },
                { WaitStatus.KeyReportHintLateGroups, "Grupos atrasados (famílias): deixados de propósito ao vanilla para ficarem juntos; são poucos comparados com muitos viajantes solo." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paragens servidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paragens com passageiros à espera: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passageiros à espera: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera média: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Passageiros atrasados ignorados hoje: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Pior paragem: nenhuma, não há passageiros à espera neste momento." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera média da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nome da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidade da pior paragem: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidade do pior waypoint: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Pista da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidade da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Média do waypoint da pior linha: {0} com {1} à espera" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} piores paragens por espera média:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | média {2} | à espera {3} | entidade paragem {4} | entidade waypoint {5} | entidade linha {6} | pista linha {7}" },
                { WaitStatus.KeyReportLateGroups, "Cims atrasados em grupo deixados em paz: {0} passageiros em {1} grupos em {2} veículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemplos de cims solo atrasados ignorados" },
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
