namespace LPRMonitoring
{
    partial class LPRMonitoring
    {
        /// <summary> 
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod wygenerowany przez Projektanta składników

        /// <summary> 
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować 
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.fileWatcherWatchDdriveArticleimagefolder = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.fileWatcherWatchDdriveArticleimagefolder)).BeginInit();
            // 
            // fileWatcherWatchDdriveArticleimagefolder
            // 
            this.fileWatcherWatchDdriveArticleimagefolder.EnableRaisingEvents = true;
            // 
            // LPRMonitoring
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.fileWatcherWatchDdriveArticleimagefolder)).EndInit();

        }

        #endregion

        private System.IO.FileSystemWatcher fileWatcherWatchDdriveArticleimagefolder;
    }
}
