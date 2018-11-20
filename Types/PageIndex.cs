namespace SharpDb
{
    public class PageIndex
    {
        private int value;

        private PageIndex(int value) => this.value = value;

        public static implicit operator PageIndex(int value) => new PageIndex(value);

        public static explicit operator int(PageIndex pageIndex) => pageIndex.value;
    }
}