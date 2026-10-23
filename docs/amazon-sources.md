# Amazon UK product source register

Observed on 20 October 2026 through normal public Amazon UK search and product pages. This is a one-time saved dataset, not a live API integration. Descriptions were independently rewritten from factual specifications and visible product form. The fictional shop brand is not presented as the manufacturer.

Product photographs are local copies of the referenced listing images. Rights remain with their respective owners; this collection does not itself establish a licence to republish. Retain this register for coursework attribution.

Prices are the GBP search-listing snapshots observed on that date and apply to the complete pack shown in each name. Amazon prices can subsequently change. Stock values are local demonstration quantities, not Amazon stock. Capacities are per vessel, not the sum of a pack. Normalized material Ceramic includes stoneware/porcelain; precise material is retained in the dataset.

| Product | Capacity per vessel | Pack | Reference GBP | Amazon source |
|---|---:|---:|---:|---|
| Ava Nord Sage Ash Mugs - Set of 4 | 340 ml | 4 | 23.71 | [B0DPGF2NX7](https://www.amazon.co.uk/dp/B0DPGF2NX7) |
| Le Creuset Volcanic Stoneware Mug | 350 ml | 1 | 17.00 | [B07MB5N887](https://www.amazon.co.uk/dp/B07MB5N887) |
| Argon Gloss Grey Mugs - Set of 6 | 350 ml | 6 | 16.49 | [B07L8JMW7D](https://www.amazon.co.uk/dp/B07L8JMW7D) |
| Sass & Belle Mojave Grey Mug | 360 ml | 1 | 9.75 | [B07YLMSFGF](https://www.amazon.co.uk/dp/B07YLMSFGF) |
| vancasso SIMI Porcelain Mugs - Set of 6 | 480 ml | 6 | 39.99 | [B0F7X26WFF](https://www.amazon.co.uk/dp/B0F7X26WFF) |
| Argon Traditional Hiball Glasses - Set of 6 | 360 ml | 6 | 12.99 | [B003WP77Z6](https://www.amazon.co.uk/dp/B003WP77Z6) |
| Argon Tondo Hiball Glasses - Set of 6 | 510 ml | 6 | 12.99 | [B018MHR366](https://www.amazon.co.uk/dp/B018MHR366) |
| Premier Luxor Glass Tumblers - Set of 4 | 360 ml | 4 | 9.95 | [B077Y5MJ6V](https://www.amazon.co.uk/dp/B077Y5MJ6V) |
| Ravenhead Mode Mixer Glasses - Set of 4 | 380 ml | 4 | 8.04 | [B01CPKA96C](https://www.amazon.co.uk/dp/B01CPKA96C) |
| Simple Modern Classic Tumbler - Lavender Mist | 710 ml | 1 | 20.19 | [B09HST5JWC](https://www.amazon.co.uk/dp/B09HST5JWC) |
| Simple Modern Classic Tumbler - Almond Birch | 710 ml | 1 | 21.64 | [B0B2TJJJDM](https://www.amazon.co.uk/dp/B0B2TJJJDM) |
| WATERSY Handle Tumbler - Rose Quartz | 1182 ml | 1 | 17.99 | [B0CNPMFPT2](https://www.amazon.co.uk/dp/B0CNPMFPT2) |
| Contigo Streeterville Handle Tumbler | 1200 ml | 1 | 10.80 | [B0D9HRP7GV](https://www.amazon.co.uk/dp/B0D9HRP7GV) |
| KETIEE Insulated Coffee Cup - Blue | 380 ml | 1 | 10.99 | [B08R1QZ7KD](https://www.amazon.co.uk/dp/B08R1QZ7KD) |
| BOTTLE BOTTLE Travel Mug - Pink | 350 ml | 1 | 13.59 | [B0D5HC811Z](https://www.amazon.co.uk/dp/B0D5HC811Z) |
| Contigo Byron SNAPSEAL - Blue Corn | 470 ml | 1 | 9.00 | [B0B7P2H383](https://www.amazon.co.uk/dp/B0B7P2H383) |
| Contigo Byron SNAPSEAL - Vivacious | 470 ml | 1 | 12.50 | [B07TRDWHXG](https://www.amazon.co.uk/dp/B07TRDWHXG) |
| GoCup Insulated Travel Mug - Dark Blue | 510 ml | 1 | 9.99 | [B07XYC37FS](https://www.amazon.co.uk/dp/B07XYC37FS) |

## Verification notes

- B01CPKA96C: Capacity uses listing title 38 cl (380 ml); overview field incorrectly states 1 ml.
- B0B2TJJJDM: Capacity uses listing title 710 ml; overview field uses a mass unit, 680 g.
- B0CNPMFPT2: 1182 ml is an approximate conversion from the stated 40 US fl oz.
- B0D5HC811Z: 304 stainless steel confirmed in the listing feature text.
- B07XYC37FS: Capacity uses listing title 510 ml; overview field uses a mass unit, 482 g.

The source image URL, SHA-256, retrieval date, packaging count and price/stock provenance are retained in `data/amazon-catalogue.json`. Original fictional catalogue records were backed up under `src/HearthCup.Web/.local/catalogue-backups/` before the guarded database replacement.

The homepage hero now uses separate Unsplash lifestyle photography, not an Amazon product image; see hero-photo-source.md.
