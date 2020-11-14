# About
This service collect data from web-sites and store it to postgresql database

# Database Scheme:
![alt text](https://github.com/fisich/NewsParserService_WIN/blob/master/Database%20Scheme.bmp?raw=true)

# SQL script:
``` CREATE TABLE "news" (
	"id" serial NOT NULL,
	"title" varchar(255) NOT NULL UNIQUE,
	"annotation" varchar(1023) NOT NULL,
	"url" varchar(255) NOT NULL UNIQUE,
	"id_source" bigint NOT NULL,
	"publication_date" timestamp with time zone NOT NULL,
	"upload_date" timestamp with time zone NOT NULL,
	CONSTRAINT "news_pk" PRIMARY KEY ("id")
) WITH (
  OIDS=FALSE
);

CREATE TABLE "news_source" (
	"id" serial NOT NULL,
	"name" varchar(63) NOT NULL UNIQUE,
	"category" varchar(63) NOT NULL,
	"url" varchar(255) NOT NULL UNIQUE,
	CONSTRAINT "news_source_pk" PRIMARY KEY ("id")
) WITH (
  OIDS=FALSE
);

ALTER TABLE "news" ADD CONSTRAINT "news_fk0" FOREIGN KEY ("id_source") REFERENCES "news_source"("id"); ```
