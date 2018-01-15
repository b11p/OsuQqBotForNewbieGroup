using System.Collections.Generic;

namespace OsuQqBot.Api
{
    class User
    {
        private User()
        {

        }

        public long Id { get; private set; }
        public string Name { get; private set; }
        //public string count300 { get; private set; }
        //public string count100 { get; private set; }
        //public string count50 { get; private set; }
        public long Tth { get; private set; }
        public int PlayCount { get; private set; }
        public long RankedScore { get; private set; }
        public long TotalScore { get; private set; }
        public int Rank { get; private set; }
        public double Level { get; private set; }
        public double PP { get; private set; }
        public double Accuracy { get; private set; }
        //public string count_rank_ss { get; private set; }
        //public string count_rank_s { get; private set; }
        //public string count_rank_a { get; private set; }
        public string Country { get; private set; }
        public int CountryRank { get; private set; }
        //public Event[] events { get; private set; }

        public static implicit operator User(UserRaw raw)
        {
            return new User
            {
                Id = long.Parse(raw.user_id),
                Name = raw.username,
                Tth = long.Parse(raw.count300) + long.Parse(raw.count100) + long.Parse(raw.count50),
                PlayCount = int.Parse(raw.playcount),
                RankedScore = long.Parse(raw.ranked_score),
                TotalScore = long.Parse(raw.total_score),
                Rank = int.Parse(raw.pp_rank),
                Level = double.Parse(raw.level),
                PP = double.Parse(raw.pp_raw),
                Accuracy = double.Parse(raw.accuracy),
                Country = countries.GetValueOrDefault(raw.country, raw.country),
                CountryRank = int.Parse(raw.pp_country_rank),
            };
        }

        private static Dictionary<string, string> countries = new Dictionary<string, string>
        {
            {"AF", "Afghanistan" },
            {"AX", "Åland Islands" },
            {"AL", "Albania" },
            {"DZ", "Algeria" },
            {"AS", "American Samoa" },
            {"AD", "Andorra" },
            {"AO", "Angola" },
            {"AI", "Anguilla" },
            {"AQ", "Antarctica" },
            {"AG", "Antigua and Barbuda" },
            {"AR", "Argentina" },
            {"AM", "Armenia" },
            {"AW", "Aruba" },
            {"AU", "Australia" },
            {"AT", "Austria" },
            {"AZ", "Azerbaijan" },
            {"BS", "Bahamas" },
            {"BH", "Bahrain" },
            {"BD", "Bangladesh" },
            {"BB", "Barbados" },
            {"BY", "Belarus" },
            {"BE", "Belgium" },
            {"BZ", "Belize" },
            {"BJ", "Benin" },
            {"BM", "Bermuda" },
            {"BT", "Bhutan" },
            {"BO", "Bolivia, Plurinational State of" },
            {"BQ", "Bonaire, Sint Eustatius and Saba" },
            {"BA", "Bosnia and Herzegovina" },
            {"BW", "Botswana" },
            {"BV", "Bouvet Island" },
            {"BR", "Brazil" },
            {"IO", "British Indian Ocean Territory" },
            {"BN", "Brunei Darussalam" },
            {"BG", "Bulgaria" },
            {"BF", "Burkina Faso" },
            {"BI", "Burundi" },
            {"KH", "Cambodia" },
            {"CM", "Cameroon" },
            {"CA", "Canada" },
            {"CV", "Cape Verde" },
            {"KY", "Cayman Islands" },
            {"CF", "Central African Republic" },
            {"TD", "Chad" },
            {"CL", "Chile" },
            {"CN", "China" },
            {"CX", "Christmas Island" },
            {"CC", "Cocos (Keeling) Islands" },
            {"CO", "Colombia" },
            {"KM", "Comoros" },
            {"CG", "Congo" },
            {"CD", "Congo, the Democratic Republic of the" },
            {"CK", "Cook Islands" },
            {"CR", "Costa Rica" },
            {"CI", "Côte d'Ivoire" },
            {"HR", "Croatia" },
            {"CU", "Cuba" },
            {"CW", "Curaçao" },
            {"CY", "Cyprus" },
            {"CZ", "Czech Republic" },
            {"DK", "Denmark" },
            {"DJ", "Djibouti" },
            {"DM", "Dominica" },
            {"DO", "Dominican Republic" },
            {"EC", "Ecuador" },
            {"EG", "Egypt" },
            {"SV", "El Salvador" },
            {"GQ", "Equatorial Guinea" },
            {"ER", "Eritrea" },
            {"EE", "Estonia" },
            {"ET", "Ethiopia" },
            {"FK", "Falkland Islands (Malvinas)" },
            {"FO", "Faroe Islands" },
            {"FJ", "Fiji" },
            {"FI", "Finland" },
            {"FR", "France" },
            {"GF", "French Guiana" },
            {"PF", "French Polynesia" },
            {"TF", "French Southern Territories" },
            {"GA", "Gabon" },
            {"GM", "Gambia" },
            {"GE", "Georgia" },
            {"DE", "Germany" },
            {"GH", "Ghana" },
            {"GI", "Gibraltar" },
            {"GR", "Greece" },
            {"GL", "Greenland" },
            {"GD", "Grenada" },
            {"GP", "Guadeloupe" },
            {"GU", "Guam" },
            {"GT", "Guatemala" },
            {"GG", "Guernsey" },
            {"GN", "Guinea" },
            {"GW", "Guinea-Bissau" },
            {"GY", "Guyana" },
            {"HT", "Haiti" },
            {"HM", "Heard Island and McDonald Islands" },
            {"VA", "Holy See (Vatican City State)" },
            {"HN", "Honduras" },
            {"HK", "Hong Kong" },
            {"HU", "Hungary" },
            {"IS", "Iceland" },
            {"IN", "India" },
            {"ID", "Indonesia" },
            {"IR", "Iran, Islamic Republic of" },
            {"IQ", "Iraq" },
            {"IE", "Ireland" },
            {"IM", "Isle of Man" },
            {"IL", "Israel" },
            {"IT", "Italy" },
            {"JM", "Jamaica" },
            {"JP", "Japan" },
            {"JE", "Jersey" },
            {"JO", "Jordan" },
            {"KZ", "Kazakhstan" },
            {"KE", "Kenya" },
            {"KI", "Kiribati" },
            {"KP", "Korea, Democratic People's Republic of" },
            {"KR", "Korea, Republic of" },
            {"KW", "Kuwait" },
            {"KG", "Kyrgyzstan" },
            {"LA", "Lao People's Democratic Republic" },
            {"LV", "Latvia" },
            {"LB", "Lebanon" },
            {"LS", "Lesotho" },
            {"LR", "Liberia" },
            {"LY", "Libya" },
            {"LI", "Liechtenstein" },
            {"LT", "Lithuania" },
            {"LU", "Luxembourg" },
            {"MO", "Macao" },
            {"MK", "Macedonia, the former Yugoslav Republic of" },
            {"MG", "Madagascar" },
            {"MW", "Malawi" },
            {"MY", "Malaysia" },
            {"MV", "Maldives" },
            {"ML", "Mali" },
            {"MT", "Malta" },
            {"MH", "Marshall Islands" },
            {"MQ", "Martinique" },
            {"MR", "Mauritania" },
            {"MU", "Mauritius" },
            {"YT", "Mayotte" },
            {"MX", "Mexico" },
            {"FM", "Micronesia, Federated States of" },
            {"MD", "Moldova, Republic of" },
            {"MC", "Monaco" },
            {"MN", "Mongolia" },
            {"ME", "Montenegro" },
            {"MS", "Montserrat" },
            {"MA", "Morocco" },
            {"MZ", "Mozambique" },
            {"MM", "Myanmar" },
            {"NA", "Namibia" },
            {"NR", "Nauru" },
            {"NP", "Nepal" },
            {"NL", "Netherlands" },
            {"NC", "New Caledonia" },
            {"NZ", "New Zealand" },
            {"NI", "Nicaragua" },
            {"NE", "Niger" },
            {"NG", "Nigeria" },
            {"NU", "Niue" },
            {"NF", "Norfolk Island" },
            {"MP", "Northern Mariana Islands" },
            {"NO", "Norway" },
            {"OM", "Oman" },
            {"PK", "Pakistan" },
            {"PW", "Palau" },
            {"PS", "Palestinian Territory, Occupied" },
            {"PA", "Panama" },
            {"PG", "Papua New Guinea" },
            {"PY", "Paraguay" },
            {"PE", "Peru" },
            {"PH", "Philippines" },
            {"PN", "Pitcairn" },
            {"PL", "Poland" },
            {"PT", "Portugal" },
            {"PR", "Puerto Rico" },
            {"QA", "Qatar" },
            {"RE", "Réunion" },
            {"RO", "Romania" },
            {"RU", "Russian Federation" },
            {"RW", "Rwanda" },
            {"BL", "Saint Barthélemy" },
            {"SH", "Saint Helena, Ascension and Tristan da Cunha" },
            {"KN", "Saint Kitts and Nevis" },
            {"LC", "Saint Lucia" },
            {"MF", "Saint Martin (French part)" },
            {"PM", "Saint Pierre and Miquelon" },
            {"VC", "Saint Vincent and the Grenadines" },
            {"WS", "Samoa" },
            {"SM", "San Marino" },
            {"ST", "Sao Tome and Principe" },
            {"SA", "Saudi Arabia" },
            {"SN", "Senegal" },
            {"RS", "Serbia" },
            {"SC", "Seychelles" },
            {"SL", "Sierra Leone" },
            {"SG", "Singapore" },
            {"SX", "Sint Maarten (Dutch part)" },
            {"SK", "Slovakia" },
            {"SI", "Slovenia" },
            {"SB", "Solomon Islands" },
            {"SO", "Somalia" },
            {"ZA", "South Africa" },
            {"GS", "South Georgia and the South Sandwich Islands" },
            {"SS", "South Sudan" },
            {"ES", "Spain" },
            {"LK", "Sri Lanka" },
            {"SD", "Sudan" },
            {"SR", "Suriname" },
            {"SJ", "Svalbard and Jan Mayen" },
            {"SZ", "Swaziland" },
            {"SE", "Sweden" },
            {"CH", "Switzerland" },
            {"SY", "Syrian Arab Republic" },
            {"TW", "Taiwan, Province of China" },
            {"TJ", "Tajikistan" },
            {"TZ", "Tanzania, United Republic of" },
            {"TH", "Thailand" },
            {"TL", "Timor-Leste" },
            {"TG", "Togo" },
            {"TK", "Tokelau" },
            {"TO", "Tonga" },
            {"TT", "Trinidad and Tobago" },
            {"TN", "Tunisia" },
            {"TR", "Turkey" },
            {"TM", "Turkmenistan" },
            {"TC", "Turks and Caicos Islands" },
            {"TV", "Tuvalu" },
            {"UG", "Uganda" },
            {"UA", "Ukraine" },
            {"AE", "United Arab Emirates" },
            {"GB", "United Kingdom" },
            {"US", "United States" },
            {"UM", "United States Minor Outlying Islands" },
            {"UY", "Uruguay" },
            {"UZ", "Uzbekistan" },
            {"VU", "Vanuatu" },
            {"VE", "Venezuela, Bolivarian Republic of" },
            {"VN", "Viet Nam" },
            {"VG", "Virgin Islands, British" },
            {"VI", "Virgin Islands, U.S." },
            {"WF", "Wallis and Futuna" },
            {"EH", "Western Sahara" },
            {"YE", "Yemen" },
            {"ZM", "Zambia" },
            {"ZW", "Zimbabwe" },
        };
    }

    class Beatmap
    {
        private Beatmap() { }

        public long Sid { get; set; }
        public long Bid { get; set; }
        //public string approved { get; set; }
        //public string total_length { get; set; }
        //public string hit_length { get; set; }
        public string Difficulty { get; set; }
        //public string MD5 { get; set; }
        public double CS { get; set; }
        public double OD { get; set; }
        public double AR { get; set; }
        public double HP { get; set; }
        //public Mode Mode { get; set; }
        //public string approved_date { get; set; }
        //public string last_update { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public double Bpm { get; set; }
        //public string source { get; set; }
        //public string tags { get; set; }
        //public string genre_id { get; set; }
        //public string language_id { get; set; }
        //public string favourite_count { get; set; }
        //public string playcount { get; set; }
        //public string passcount { get; set; }
        //public string max_combo { get; set; }
        public double Stars { get; set; }

        public static implicit operator Beatmap(beatmap raw)
        {
            return new Beatmap
            {
                Sid = long.Parse(raw.beatmapset_id),
                Bid = long.Parse(raw.beatmap_id),
                Difficulty = raw.version,
                CS = double.Parse(raw.diff_size),
                OD = double.Parse(raw.diff_overall),
                AR = double.Parse(raw.diff_approach),
                HP = double.Parse(raw.diff_drain),
                Artist = raw.artist,
                Title = raw.title,
                Creator = raw.creator,
                Bpm = double.Parse(raw.bpm),
                Stars = double.Parse(raw.difficultyrating),
            };
        }
    }

    class BestPerformance
    {
        private BestPerformance() { }

        private Beatmap _beatmap;

        public Beatmap Beatmap
        {
            get => _beatmap;
            set => _beatmap = value;
        }

        public long BeatmapId { get; private set; }
        public long Score { get; private set; }
        public int Combo { get; private set; }
        public int Count300 { get; private set; }
        public int Count100 { get; private set; }
        public int Count50 { get; private set; }
        public int CountMiss { get; private set; }
        public int CountKatu { get; private set; }
        public int CountGeki { get; private set; }
        public bool FullCombo { get; private set; }
        public OsuApiClient.Mods EnabledMods { get; private set; }
        public long UserId { get; private set; }
        //public string Date { get; private set; }
        public string Rank { get; private set; }
        public double PP { get; private set; }

        public static implicit operator BestPerformance(best_performance bp)
        {
            return new BestPerformance
            {
                BeatmapId = long.Parse(bp.beatmap_id),
                Score = long.Parse(bp.score),
                Combo = int.Parse(bp.maxcombo),
                Count300 = int.Parse(bp.count300),
                Count100 = int.Parse(bp.count100),
                Count50 = int.Parse(bp.count50),
                CountMiss = int.Parse(bp.countmiss),
                CountKatu = int.Parse(bp.countkatu),
                CountGeki = int.Parse(bp.countgeki),
                FullCombo = bp.perfect == "1",
                EnabledMods = (OsuApiClient.Mods)int.Parse(bp.enabled_mods),
                UserId = long.Parse(bp.user_id),
                //Date = 
                Rank = bp.rank,
                PP = double.Parse(bp.pp),
            };
        }
    }
}