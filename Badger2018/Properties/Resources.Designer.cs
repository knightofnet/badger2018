﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Badger2018.Properties {
    using System;
    
    
    /// <summary>
    ///   Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Badger2018.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Remplace la propriété CurrentUICulture du thread actuel pour toutes
        ///   les recherches de ressources à l'aide de cette classe de ressource fortement typée.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap autoBadgeMidiOff {
            get {
                object obj = ResourceManager.GetObject("autoBadgeMidiOff", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap autoBadgeMidiOn {
            get {
                object obj = ResourceManager.GetObject("autoBadgeMidiOn", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap autoStartOff {
            get {
                object obj = ResourceManager.GetObject("autoStartOff", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap autoStartOn {
            get {
                object obj = ResourceManager.GetObject("autoStartOn", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à BEGIN TRANSACTION;
        ///DROP TABLE IF EXISTS `TYBADGEAGES`;
        ///CREATE TABLE IF NOT EXISTS `TYBADGEAGES` (
        ///	`TYPE_BADGE`	INTEGER,
        ///	`BADGE_NAME`	TEXT,
        ///	PRIMARY KEY(`TYPE_BADGE`)
        ///);
        ///INSERT INTO `TYBADGEAGES` VALUES (1,&apos;PLAGE_TRAV_MATIN_START&apos;);
        ///INSERT INTO `TYBADGEAGES` VALUES (2,&apos;PLAGE_TRAV_MATIN_END&apos;);
        ///INSERT INTO `TYBADGEAGES` VALUES (3,&apos;PLAGE_TRAV_APREM_START&apos;);
        ///INSERT INTO `TYBADGEAGES` VALUES (4,&apos;PLAGE_TRAV_APREM_END&apos;);
        ///INSERT INTO `TYBADGEAGES` VALUES (10,&apos;PAUSE_START&apos;);
        ///INSERT INTO `TYBADGEAGES` VA [le reste de la chaîne a été tronqué]&quot;;.
        /// </summary>
        internal static string dbbCreate {
            get {
                return ResourceManager.GetString("dbbCreate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à app.db.
        /// </summary>
        internal static string dbbFile {
            get {
                return ResourceManager.GetString("dbbFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;root&gt;
        ///  &lt;elt&gt;
        ///    Un licence à renouveler
        ///    Afin de protéger l&apos;outil et éviter sa diffusion de façon trop large, chaque utilisateur dispose d&apos;une licence. Cette dernière est à renouveler environ tous les deux ans.
        ///    Pour constulter les informations de votre licence, appuyer sur la touche F8 dans la fenêtre principale.
        ///    Quelques jours avant l&apos;expiration de votre licence, vous serez averti à chaque démarrage de l&apos;outil par un message.
        ///  &lt;/elt&gt;
        ///  &lt;elt&gt;
        ///    Comment me rapp [le reste de la chaîne a été tronqué]&quot;;.
        /// </summary>
        internal static string didyouknow {
            get {
                return ResourceManager.GetString("didyouknow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à &lt;ConfigFile&gt;
        ///  &lt;IsUseGeckoDebug&gt;False&lt;/IsUseGeckoDebug&gt; 
        ///&lt;/ConfigFile&gt;.
        /// </summary>
        internal static string DisabledGeckoDebug {
            get {
                return ResourceManager.GetString("DisabledGeckoDebug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à &lt;ConfigFile&gt;
        ///  &lt;IsUseGeckoDebug&gt;True&lt;/IsUseGeckoDebug&gt; 
        ///&lt;/ConfigFile&gt;.
        /// </summary>
        internal static string EnableGeckoDebug {
            get {
                return ResourceManager.GetString("EnableGeckoDebug", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap firefox_light_016 {
            get {
                object obj = ResourceManager.GetObject("firefox_light_016", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap fondGta {
            get {
                object obj = ResourceManager.GetObject("fondGta", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap font_awesome_4_7_0_external_link_23_0_3498db_none {
            get {
                object obj = ResourceManager.GetObject("font_awesome_4_7_0_external_link_23_0_3498db_none", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à &lt;html&gt;
        ///&lt;head&gt;
        ///    &lt;title&gt;Aide du programme&lt;/title&gt;
        ///
        ///    &lt;meta charset=&quot;UTF-8&quot;&gt;
        ///    &lt;meta http-equiv=&quot;X-UA-Compatible&quot; content=&quot;IE=edge&quot;&gt;
        ///
        ///    &lt;style&gt;
        ///        * {
        ///            font-family: &quot;segoe ui&quot;, verdana, arial, sans-serif;
        ///            box-sizing: border-box;
        ///            -webkit-box-sizing: border-box;
        ///            -moz-box-sizing: border-box;
        ///        }
        ///
        ///        p, li {
        ///            text-align: justify;
        ///        }
        ///
        ///
        ///        h2 {
        ///            font-size: 1.65em;
        ///            display: block [le reste de la chaîne a été tronqué]&quot;;.
        /// </summary>
        internal static string help {
            get {
                return ResourceManager.GetString("help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap helpIcon {
            get {
                object obj = ResourceManager.GetObject("helpIcon", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap iconOkHover {
            get {
                object obj = ResourceManager.GetObject("iconOkHover", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap iconSetting {
            get {
                object obj = ResourceManager.GetObject("iconSetting", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap iconWarning {
            get {
                object obj = ResourceManager.GetObject("iconWarning", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap notifOff {
            get {
                object obj = ResourceManager.GetObject("notifOff", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap notifOn {
            get {
                object obj = ResourceManager.GetObject("notifOn", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Icon semblable à (Icône).
        /// </summary>
        internal static System.Drawing.Icon Paomedia_Small_N_Flat_Clock {
            get {
                object obj = ResourceManager.GetObject("Paomedia_Small_N_Flat_Clock", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap pauseBlinkOn {
            get {
                object obj = ResourceManager.GetObject("pauseBlinkOn", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap pauseExists {
            get {
                object obj = ResourceManager.GetObject("pauseExists", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap planTab {
            get {
                object obj = ResourceManager.GetObject("planTab", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap sign_error_icon {
            get {
                object obj = ResourceManager.GetObject("sign_error_icon", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap sign_info_icon {
            get {
                object obj = ResourceManager.GetObject("sign_info_icon", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap sign_warning_icon {
            get {
                object obj = ResourceManager.GetObject("sign_warning_icon", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;package&gt;
        ///  &lt;release&gt;
        ///    &lt;version&gt;1.0.801.517&lt;/version&gt;
        ///    &lt;title&gt;Bravo Barracuda&lt;/title&gt;
        ///    &lt;authors&gt;Arnaud Leblanc&lt;/authors&gt;
        ///    &lt;description&gt;Version initiale&lt;/description&gt;
        ///    &lt;levelUpdate&gt;0&lt;/levelUpdate&gt;
        ///    &lt;needIntermediateLaunch&gt;True&lt;/needIntermediateLaunch&gt;
        ///    &lt;fileUpdate&gt;E:\CSharp\DonnÃ©es accessoires\Badger\badgerA.exe&lt;/fileUpdate&gt;
        ///  &lt;/release&gt;
        ///  &lt;release&gt;
        ///    &lt;version&gt;1.0.0826.1723&lt;/version&gt;
        ///    &lt;title&gt;Bravo Barracuda&lt;/title&gt;
        ///    &lt;authors&gt;Arnaud Leblanc&lt;/au [le reste de la chaîne a été tronqué]&quot;;.
        /// </summary>
        internal static string update {
            get {
                return ResourceManager.GetString("update", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap updateIcon {
            get {
                object obj = ResourceManager.GetObject("updateIcon", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Caesium Canberra.
        /// </summary>
        internal static string versionName {
            get {
                return ResourceManager.GetString("versionName", resourceCulture);
            }
        }
    }
}
