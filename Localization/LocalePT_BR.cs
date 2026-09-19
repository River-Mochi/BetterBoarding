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

        /// <summary>
        /// Constructs the Brazilian Portuguese locale.
        /// </summary>
        /// <param name="setting">Settings object used for locale IDs.</param>
        public LocalePT_BR(BBoardSettings setting)
        {
            m_Setting = setting;
        }

        /// <summary>
        /// Creates all Brazilian Portuguese localization entries for this mod.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;

            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            const string ToggleName = "Pular passageiros atrasados";

            string SpeedDescription(string transitName, string shortName, string extraLine)
            {
                return
                    "<1x = vanilla>\n" +
                    extraLine +
                    $"Valores maiores reduzem o tempo de embarque e carregamento em {transitName}.\n" +
                    $"3x é o padrão recomendado.\n" +
                    $"5x é o máximo.\n" +
                    $"Isso ajuda filas normais a andar mais rápido, mas um passageiro atrasado ainda pode atrasar a partida por causa do design vanilla.\n" +
                    $"Use [✓] <{ToggleName}> se quiser que cims atrasados percam o veículo depois do horário de partida.\n" +
                    $"Cidadãos atrasados pulados não são excluídos; o jogo os redireciona naturalmente.\n" +
                    "<==========================>\n" +
                    "Valor de carregamento:\n" +
                    "1x = 100% parada vanilla\n" +
                    "2x = ~1/2 da parada planejada\n" +
                    "3x = ~1/3 da parada planejada (recomendado)\n" +
                    "5x = ~1/5 da parada planejada (máx.)\n" +
                    $"Isso não é a mesma coisa que <{ToggleName}>; essa caixa decide se cims atrasados podem perder o {shortName} depois do horário de partida.";
            }

            // One helper keeps all seven status tooltips in sync for future translations.
            string StatusDescription(string transitName)
            {
                return
                    $"<Status atual de {transitName}>\n" +
                    "**Esperando** = total de passageiros esperando agora.\n" +
                    "**Média** = tempo médio de espera desses passageiros.\n" +
                    "**Pior** parada = maior espera média em uma parada.\n" +
                    "As piores paradas são bons lugares para procurar acidentes, paradas bloqueadas/bugadas ou necessidade de mais veículos atribuídos.\n" +
                    $"**Atrasados hoje** = passageiros solo atrasados pulados hoje por <{ToggleName}>.\n" +
                    "Use <Stats para log> para relatório detalhado: nomes de paradas, IDs de entidades e mais.";
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)), "Velocidade do ônibus" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.BusBoardingSpeedFactor)),
                    SpeedDescription(
                        "ponto de ônibus",
                        "ônibus",
                        string.Empty)
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)), "Velocidade ferroviária" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.RailBoardingSpeedFactor)),
                    SpeedDescription(
                        "parada de trem, bonde e metrô",
                        "veículo",
                        "Aplica-se a paradas de trem, bonde e metrô.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)), "Navio + balsa" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.WaterBoardingSpeedFactor)),
                    SpeedDescription(
                        "parada de navio e balsa",
                        "veículo",
                        "Aplica-se a paradas de navio e balsa.\n")
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)), "Velocidade do avião" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AirBoardingSpeedFactor)),
                    SpeedDescription(
                        "terminal de avião",
                        "avião",
                        "Aplica-se a terminais de avião de passageiros.\n")
                },

                // Late passenger behavior
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CancelLateBoarders)), ToggleName },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CancelLateBoarders)),
                    "Passageiros atrasados que ainda estão <não prontos> depois do horário de partida podem perder o veículo.\n" +
                    "Nota: pulamos apenas cidadãos solo atrasados.\n" +
                    "Grupos/famílias viajando juntos que estão atrasados <não são pulados> e ainda podem causar atrasos como no vanilla.\n" +
                    "Grupos são uma pequena parte da multidão; a maior parte do benefício vem de pular cims solo atrasados.\n" +
                    "Cidadãos atrasados pulados não são excluídos; eles são naturalmente reatribuídos pelo jogo."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)), "Cims correm antes: ônibus + bondes + trens" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.CimsRunSoonerToCatchBuses)),
                    "Cidadãos <atrasados> começam a <correr antes> para tentar chegar **antes** da hora de partida.\n" +
                    "Ajuda ônibus, bondes e trens a manterem o horário, especialmente em plataformas longas.\n" +
                    "Afeta apenas cims já atribuídos a um veículo que está embarcando.\n" +
                    "No vanilla, os cims só começam a correr na hora de partida, o que pode ser tarde demais.\n" +
                    $"Combina bem com <{ToggleName}> porque pode reduzir quantos cims perdem o veículo e precisam ser reatribuídos.\n" +
                    "Não altera a hora de partida do veículo, não força embarque nem teleporta cidadãos."
                },

                // Status overview
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusOverview)), "Uso total" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusOverview)),
                    "Uso mensal do transporte público a partir da infoview Transporte do jogo.\n" +
                    "O horário atualizado mostra quando este snapshot de status foi tirado (geralmente depois de entrar no menu Opções)."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)), "Cims correm antes" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatusCimsRunSooner)),
                    "Se ativado [x], conta todos os cims (hoje) que começaram a **correr antes** para tentar pegar um ônibus, bonde ou trem antes da partida.\n" +
                    "Os cims correm 512 frames antes do vanilla (~2-8 segundos antes em tempo real, ~2 minutos no jogo)."
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
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.StatsToLog)), "Stats para log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.StatsToLog)),
                    "Escreve um relatório detalhado único em **BetterBoarding.log**.\n" +
                    "Inclui totais de espera, top 3 piores paradas por modo, exemplos de cims pulados, IDs de entidades e dicas de linha."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenLog)), "Abrir log" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenLog)),
                    "Abre **BetterBoarding.log** se existir.\n" +
                    "Se o arquivo ainda não for encontrado, abre a pasta Logs."
                },

                // About
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutName)), "Mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutName)), "Nome exibido deste mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.AboutVersion)), "Versão atual do mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.OpenParadoxMods)), "Abre a página do autor no Paradox Mods." },

                // Debug
                { m_Setting.GetOptionLabelLocaleID(nameof(BBoardSettings.EnableVerboseLogging)), "Ativar log detalhado" },
                { m_Setting.GetOptionDescLocaleID(nameof(BBoardSettings.EnableVerboseLogging)),
                    "**Somente debug / teste**\n" +
                    "Adiciona detalhes <live> a <Logs/BetterBoarding.log> enquanto a cidade roda.\n" +
                    "**Não ative para jogo normal.**\n" +
                    "Deixar isso ligado pode reduzir o desempenho e criar arquivos de log enormes.\n" +
                    "Você pode excluir logs antigos depois.\n" +
                    "Nota: <Stats para log> é um relatório pontual com os contadores de atrasados pulados de hoje; é diferente dos logs detalhados.\n" +
                    "Execute o log detalhado por 15-20 min se quiser uma linha do tempo do que aconteceu.\n" +
                    "Só não esqueça de voltar para **OFF** antes do jogo normal."
                },

                // Runtime status strings
                { WaitStatus.KeyStatusNotLoaded, "Status não carregado." },
                { WaitStatus.KeyNoCityLoaded, "Nenhuma cidade carregada." },
                { WaitStatus.KeyNoStopsFound, "Nenhuma parada encontrada." },

                { WaitStatus.KeyStatusLine, "{0} esperando | méd. {1} | pior {2} | {3}" },
                { WaitStatus.KeyStatusLateSkipped, "{0} atrasados hoje" },
                { WaitStatus.KeyStatusSkipOff, "skip OFF" },

                { WaitStatus.KeyStatusOverviewLine, "{0} turistas/mês | {1} cidadãos/mês | atualizado {2}" },
                { WaitStatus.KeyStatusRunSoonerLine, "{0}" },
                { WaitStatus.KeyStatusRunSoonerOff, "correr antes OFF" },

                // Stats-to-log report strings
                { WaitStatus.KeyReportNoCityLoaded, "[BBoard] Relatório solicitado, mas nenhuma cidade está carregada." },
                { WaitStatus.KeyReportTitle, "Snapshot Stats para log - Better Boarding" },
                { WaitStatus.KeyReportSettings, "Configurações: {0}" },
                { WaitStatus.KeyReportNote, "A dica de linha vem do waypoint com maior espera nessa parada." },
                { WaitStatus.KeyReportTesterHintsHeader, "Dicas para testes" },
                { WaitStatus.KeyReportHintWorstStops, "Piores paradas: inspecione primeiro no jogo ou com o mod Scene Explorer (encontre locais pelo ID da entidade). Procure trânsito, local ruim da parada ou parada bugada." },
                { WaitStatus.KeyReportHintSkippedCims, "Cims solo pulados: passageiros atrasados que pulamos para permitir que o transporte saia. Depois, o estado normalmente deve virar 'has path' ou 'assigned'. Se ficar em 'no path yet', inspecione essa entidade cim depois de mais tempo." },
                { WaitStatus.KeyReportHintLateGroups, "Grupos atrasados (famílias): deixados intencionalmente no vanilla para ficarem juntos e seguirem o comportamento vanilla; são poucos comparados a muitos viajantes sozinhos." },
                { WaitStatus.KeyReportFamilyHeader, "{0}" },
                { WaitStatus.KeyReportServedStops, "Paradas atendidas: {0}" },
                { WaitStatus.KeyReportStopsWithWaiting, "Paradas com passageiros esperando: {0}" },
                { WaitStatus.KeyReportWaitingPassengers, "Passageiros esperando: {0}" },
                { WaitStatus.KeyReportAverageWait, "Espera média: {0}" },
                { WaitStatus.KeyReportLateBoardersSkipped, "Passageiros atrasados pulados hoje: {0}" },
                { WaitStatus.KeyReportWorstStopNone, "Pior parada: nenhuma, não há passageiros esperando agora." },
                { WaitStatus.KeyReportWorstStopAverageWait, "Espera média da pior parada: {0}" },
                { WaitStatus.KeyReportWorstStopName, "Nome da pior parada: {0}" },
                { WaitStatus.KeyReportWorstStopEntity, "Entidade da pior parada: {0}" },
                { WaitStatus.KeyReportWorstWaypointEntity, "Entidade waypoint da pior parada: {0}" },
                { WaitStatus.KeyReportWorstLineHint, "Dica da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineEntity, "Entidade da pior linha: {0}" },
                { WaitStatus.KeyReportWorstLineWaypointAverage, "Média do waypoint da pior linha: {0} com {1} esperando" },
                { WaitStatus.KeyReportTopWorstStopsHeader, "Top {0} piores paradas por espera média:" },
                { WaitStatus.KeyReportTopWorstStopLine, "{0}. {1} | méd. {2} | esperando {3} | parada {4} | waypoint {5} | linha {6} | dica {7}" },
                { WaitStatus.KeyReportLateGroups, "Cims atrasados viajando em grupo deixados em paz: {0} passageiros em {1} grupos em {2} veículos" },
                { WaitStatus.KeyReportLastSkippedSamplesHeader, "Exemplos de cims solo atrasados pulados" },
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
