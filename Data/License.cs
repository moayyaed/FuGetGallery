﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FuGetGallery
{
    public class License
    {
        public string Name { get; set; } = "";
        public bool AllowsDecompilation { get; set; }
        public HashSet<string> KnownUrls { get; set; } = new HashSet<string> ();

        string templateName = "";
        public string TemplateName {
            get => templateName;
            set { templateName = value; LoadTemplate (); }
        }

        public string TemplateText { get; private set; }
        public HashSet<string> TemplateBigrams { get; private set; }

        readonly static List<License> known = new List<License> ();

        static License ()
        {
            known.Add (new License {
                Name = "Apache License 2.0",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://www.apache.org/licenses/LICENSE-2.0",
                    "https://www.apache.org/licenses/LICENSE-2.0",
                    "http://opensource.org/licenses/Apache-2.0",
                    "https://opensource.org/licenses/Apache-2.0",
                },
                TemplateName = "Apache2",
            });

            known.Add (new License {
                Name = "3-Clause BSD License",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://opensource.org/licenses/BSD-3-Clause",
                    "https://opensource.org/licenses/BSD-3-Clause",
                },
                TemplateName = "BSD3Clause",
            });

            known.Add (new License {
                Name = "2-Clause BSD License",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://opensource.org/licenses/BSD-2-Clause",
                    "https://opensource.org/licenses/BSD-2-Clause",
                },
                TemplateName = "BSD2Clause",
            });

            known.Add (new License {
                Name = "GNU General Public License 2",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://www.gnu.org/licenses/gpl-2.0.html",
                    "https://www.gnu.org/licenses/gpl-2.0.html",
                    "http://opensource.org/licenses/GPL-2.0",
                    "https://opensource.org/licenses/GPL-2.0",
                },
                TemplateName = "GPL2",
            });

            known.Add (new License {
                Name = "GNU General Public License 3",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://www.gnu.org/licenses/gpl-3.0.html",
                    "https://www.gnu.org/licenses/gpl-3.0.html",
                    "http://opensource.org/licenses/GPL-3.0",
                    "https://opensource.org/licenses/GPL-3.0",
                },
                TemplateName = "GPL3",
            });

            known.Add (new License {
                Name = "GNU Lesser General Public License 2.1",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://www.gnu.org/licenses/lgpl-2.1.html",
                    "https://www.gnu.org/licenses/lgpl-2.1.html",
                    "https://opensource.org/licenses/LGPL-2.1",
                    "https://opensource.org/licenses/LGPL-2.1",
                },
                TemplateName = "LGPL21",
            });

            known.Add (new License {
                Name = "MIT License",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://opensource.org/licenses/MIT",
                    "https://opensource.org/licenses/MIT",
                },
                TemplateName = "MIT",
            });

            known.Add (new License {
                Name = "MS-PL",
                AllowsDecompilation = true,
                KnownUrls = {
                    "http://opensource.org/licenses/MS-PL",
                    "https://opensource.org/licenses/MS-PL",
                },
                TemplateName = "MSPL",
            });
        }

        public static License FindLicenseWithUrl (string url)
        {
            return known.FirstOrDefault (x => x.KnownUrls.Contains (url));
        }

        public static License FindLicenseWithText (string text)
        {
            var bigrams = BuildBigramSet (text);
            var q =
                from l in known
                let dice = DiceCoefficient (l.TemplateBigrams, bigrams)
                where dice > 0.9
                orderby dice descending
                select Tuple.Create (dice, l);
            var r = q.FirstOrDefault ();
            Console.WriteLine (r);
            return r?.Item2;
        }

        void LoadTemplate ()
        {
            using (var s = GetType ().Assembly.GetManifestResourceStream ($"FuGetGallery.Resources.Licenses.{TemplateName}.txt")) {
                if (s == null) {
                    throw new Exception ("Missing license " + TemplateName);
                }
                using (var r = new StreamReader (s)) {
                    TemplateText = r.ReadToEnd ();
                    TemplateBigrams = BuildBigramSet (TemplateText);
                }
            }
        }

        /// <summary>
        /// From https://gist.github.com/ssajous/3539848
        /// </summary>
        static HashSet<string> BuildBigramSet (string input)
        {
            HashSet<string> bigrams = new HashSet<string> ();
            for (int i = 0; i < input.Length - 1; i++) {
                bigrams.Add (input.Substring (i, 2));
            }
            return bigrams;
        }

        /// <summary>
        /// From https://gist.github.com/ssajous/3539848
        /// </summary>
        public static double DiceCoefficient (HashSet<string> x, HashSet<string> y)
        {
            HashSet<string> intersection = new HashSet<string> (x);
            intersection.IntersectWith (y);
            return (2.0 * intersection.Count) / (x.Count + y.Count);
        }
    }
}