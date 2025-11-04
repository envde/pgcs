CREATE TRIGGER trigger_update_category_search
    BEFORE INSERT OR UPDATE OF name, description
                     ON categories
                         FOR EACH ROW
                         EXECUTE FUNCTION update_category_search_vector();