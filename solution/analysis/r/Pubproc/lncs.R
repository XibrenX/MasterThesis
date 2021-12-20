install.packages("RPostgres")
install.packages("languageserver")

library(DBI)
source("params.R")

con <- dbConnect(
  RPostgres::Postgres(), 
  dbname = POSTGRES_DB, 
  host=POSTGRES_SERVER,
  port=5432, 
  user=POSTGRES_USER, 
  password=POSTGRES_PASSWORD,
  sslmode = 'require',
  )
dat <- dbGetQuery(con, "SELECT * FROM analysis.analysis_lncs_members")
dbDisconnect(con)

boxplot(dat$cnt_published, breaks = sqrt(nrow(dat)))

plot(dat$cnt_cited, dat$cnt_published)

boxplot(dat$cnt_published ~ dat$book_dblp_key)
