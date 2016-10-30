namespace NuGetManager
{
    using System;

    public class Package : Observable
    {
        private bool isListed;

        public string Name { get; set; }
        public string Source { get; set; }
        public string Version { get; set; }
        public bool IsSelected { get; set; }

        public bool IsListed
        {
            get
            {
                return this.isListed;
            }

            set
            {
                this.SetValue(ref this.isListed, value);
            }
        }


        public bool IsReleaseVersion { get; set; }

        public Uri IconUrl { get; set; }

        public override string ToString()
        {
            return this.Name + " " + this.Version;
        }


    }
}