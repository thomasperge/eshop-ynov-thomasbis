-- Script pour ajouter le champ Stock aux produits existants dans Marten
-- À exécuter dans DataGrip sur la base CatalogDb

-- Pour chaque produit, on met à jour le JSON pour ajouter le champ Stock à 50 par défaut
-- (ou vous pouvez ajuster la valeur selon vos besoins)

UPDATE mt_doc_product 
SET data = jsonb_set(
    data::jsonb, 
    '{Stock}', 
    '50', 
    true
) 
WHERE data::jsonb ? 'Stock' = false OR (data::jsonb->>'Stock') IS NULL;
